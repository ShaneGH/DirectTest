using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Dynamox.Compile.ILBuilders;
using Dynamox.Mocks;

using AssemblyBuilder = System.Reflection.Emit.AssemblyBuilder;
using ModuleBuilder = System.Reflection.Emit.ModuleBuilder;
using AssemblyBuilderAccess = System.Reflection.Emit.AssemblyBuilderAccess;
using TypeBuilder = System.Reflection.Emit.TypeBuilder;
using Dynamox.Mocks.Info;

namespace Dynamox.Compile
{
    public class Compiler
    {
        private readonly ConcurrentDictionary<Type, Type> Built = new ConcurrentDictionary<Type, Type>();
        const string _ObjectBase = "_ObjectBase";
        private const string RootNamespace = "Dynamox.Proxies";
        private const string UnderlyingObject = "__DynamoxTests_BaseObject";
        public static readonly BindingFlags AllMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        static readonly Compiler Instance = new Compiler();
        public static Type Compile(Type baseType)
        {
            return Instance._Compile(baseType);
        }

        readonly AssemblyBuilder Assembly;
        readonly ModuleBuilder Module;

        private Compiler()
        {
            Assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(RootNamespace), AssemblyBuilderAccess.Run);
            Module = Assembly.DefineDynamicModule(RootNamespace);
        }

        Type _Compile(Type baseType)
        {
            if (!Built.ContainsKey(baseType))
            {
                CompileAndCache(baseType);
            }

            return Built[baseType];
        }

        readonly Dictionary<Type, Thread> Compiling = new Dictionary<Type, Thread>();
        void CompileAndCache(Type baseType)
        {
            Exception compileException = null;
            Thread compiling = null;
            lock (Compiling)
            {
                if (Built.ContainsKey(baseType)) return;

                if (Compiling.ContainsKey(baseType))
                {
                    compiling = Compiling[baseType];
                }
                else
                {
                    Compiling.Add(baseType, compiling = new Thread(() =>
                    {
                        try
                        {
                            var built = BuildType(baseType);
                            lock (Compiling)
                            {
                                Built.TryAdd(baseType, built);
                                Compiling.Remove(baseType);
                            }
                        }
                        catch (Exception e)
                        {
                            compileException = e;
                        }
                    }));
                    compiling.Start();
                }
            }

            compiling.Join();
            if (compileException != null)
                throw new CompilerException(baseType, "See inner exception for details.", compileException);

            if (!Built.ContainsKey(baseType))
                throw new CompilerException(baseType, "See previous exception for more details.");
        }

        bool AreEqual<T>(IEnumerable<T> array1, IEnumerable<T> array2)
        {
            var a1 = array1.ToArray();
            var a2 = array2.ToArray();

            if (a1.Length != a2.Length)
                return false;

            for (var i = 0; i < a1.Length; i++)
                if (!object.Equals(a1[i], a2[i]))
                    return false;

            return true;
        }

        // need to add an increment for nested classes
        static int TypeIncrement = new Random().Next(99999);
        Type BuildType(Type baseType)
        {
            if (baseType.IsNestedPrivate)
                throw new CompilerException(baseType, "You cannot mock a nested private class or interface");

            if (baseType.IsNestedAssembly)
                throw new CompilerException(baseType, "You cannot mock a nested internal class or interface");

            var typeDescriptor = TypeOverrideDescriptor.Create(baseType);
            if (typeDescriptor.HasAbstractInternal)
                throw new CompilerException(baseType, "You cannot mock a class with an internal abstract member. Method(s) found: " +
                    string.Join(Environment.NewLine, typeDescriptor.AbstractInternalMethods.Select(m => m.Name)));

            var interfaces = typeDescriptor.OverridableInterfaces
                .Select(i => i.Interface)
                .Union(new[] { typeof(IEnsure) });

            // define type
            var type = Module.DefineType(
                "Dynamox.Proxy." + baseType.Namespace + "." + baseType.Name + "_" + (++TypeIncrement),
                TypeAttributes.Public | TypeAttributes.Class,
                typeDescriptor.Type,
                interfaces.ToArray());

            var allMembers = baseType.GetMembers(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(m => m.Name).ToArray();

            // store raise event methods
            var eventHandlerFields = new List<FieldInfo>();

            // define ObjectBase field
            var objBase = type.DefineField(GetFreeMemberName(baseType, UnderlyingObject),
                typeof(ObjectBase), FieldAttributes.NotSerialized | FieldAttributes.Private | FieldAttributes.InitOnly);

            // add properties
            foreach (var property in typeDescriptor.OverridableProperties)
            {
                AddProperty(type, objBase, property);
            }

            // add methods
            foreach (var method in typeDescriptor.OverridableMethods)
            {
                var builder = method.IsAbstract ?
                    (method.ReturnType == typeof(void) ?
                        (MethodBuilder)new AbstractMethodBuilderNoReturn(type, objBase, method) :
                        new AbstractMethodBuilderWithReturn(type, objBase, method)) :
                    (method.ReturnType == typeof(void) ?
                        (MethodBuilder)new VirtualMethodBuilderNoReturn(type, objBase, method) :
                        new VirtualMethodBuilderWithReturn(type, objBase, method));

                builder.Build();
            }

            // add events
            foreach (var @event in typeDescriptor.OverridableEvents)
            {
                var eventBuilder = new EventBuilder(type, objBase, @event);
                eventBuilder.Build();
                if (eventBuilder.EventField != null)
                    eventHandlerFields.Add(eventBuilder.EventField);
            }

            // add interface properties
            foreach (var property in typeDescriptor.OverridableInterfaces.SelectMany(i => i.OverridableProperties))
            {
                //TODO: if property signature is available

                AddProperty(type, objBase, property, typeDescriptor);
            }

            // add IEnsure.ShouldHaveBeenCalled property
            if (!typeDescriptor.OverridableInterfaces.Any(i => typeof(IEnsure).IsAssignableFrom(i.Interface)))
            {
                AddIEnsureShouldHaveBeenCalled(type, objBase);
            }

            // add interface methods
            foreach (var method in typeDescriptor.OverridableInterfaces.SelectMany(i => i.OverridableMethods))
            {
                var builder = (method.ReturnType == typeof(void) ?
                    (MethodBuilder)new AbstractMethodBuilderNoReturn(type, objBase, method) :
                    new AbstractMethodBuilderWithReturn(type, objBase, method));

                builder.AddInterfaceMethodsExplicitly = typeDescriptor.MethodClashes(method);
                builder.Build();
            }

            // add interface events
            foreach (var @event in typeDescriptor.OverridableInterfaces.SelectMany(i => i.OverridableEvents))
            {
                var eventBuilder = new EventBuilder(type, objBase, @event);
                eventBuilder.Build();
                if (eventBuilder.EventField != null)
                    eventHandlerFields.Add(eventBuilder.EventField);
            }

            var implementEventInterfaces = eventHandlerFields.Any();
            if (implementEventInterfaces)
            {
                new RaiseEventMethodBuilder(type, objBase, eventHandlerFields).Build();
            }

            // add constructors
            foreach (var constructor in typeDescriptor.Type.GetConstructors(AllMembers)
                .Where(c => !c.IsAssembly || c.IsFamilyOrAssembly)
                .Select(c => new ConstructorBuilder(type, objBase, c, typeDescriptor, implementEventInterfaces)))
            {
                constructor.Build();
            }

            return type.CreateType();
        }

        public static bool IsDxCompiledType(Type type)
        {
            return Instance.Built.Any(b => b.Value == type);
        }

        static string GetFreeMemberName(Type forType, string nameBase)
        {
            var allNames = new HashSet<string>(forType.AllClassesAndInterfaces()
                .SelectMany(c => c.GetMembers(AllMembers | BindingFlags.Static))
                .Select(m => m.Name));

            string output = nameBase;
            for (var number = 1; allNames.Contains(output); number++)
                output = nameBase + number;

            return output;
        }

        internal static void AddIEnsureShouldHaveBeenCalled(TypeBuilder toType, FieldInfo objBase)
        {
            var property = toType.DefineProperty(IEnsureShouldHaveBeenCalledGetterBuilder.Name, PropertyAttributes.None, IEnsureShouldHaveBeenCalledGetterBuilder.PropertyType, null);
            var builder = new IEnsureShouldHaveBeenCalledGetterBuilder(toType, objBase);
            builder.Build();
            property.SetGetMethod(builder.Method);
        }

        internal static void AddProperty(TypeBuilder toType, FieldInfo objBase, PropertyInfo parentProperty, TypeOverrideDescriptor typeDescriptor = null)
        {
            if (!parentProperty.IsAbstract() && !parentProperty.IsVirtual())
                throw new CompilerException(toType.BaseType, "Cannot mock non virtual property " + parentProperty.Name);

            var parameterTypes = parentProperty.GetIndexParameters().Select(pt => pt.ParameterType).ToArray();
            var property = toType.DefineProperty(parentProperty.Name, PropertyAttributes.None, parentProperty.PropertyType, parameterTypes.Any() ? parameterTypes : null);

            if (parentProperty.GetMethod != null &&
                (parentProperty.GetMethod.IsAbstract || parentProperty.GetMethod.IsVirtual) &&
                !parentProperty.GetMethod.IsPrivate && !parentProperty.GetMethod.IsAssembly)
            {
                var builder = parameterTypes.Any() ?
                    (parentProperty.GetMethod.IsAbstract ?
                        new AbstractIndexGetterBuilder(toType, objBase, parentProperty) as MethodBuilder :
                        new VirtualIndexGetterBuilder(toType, objBase, parentProperty)) :
                    (parentProperty.GetMethod.IsAbstract ?
                        new AbstractPropertyGetterBuilder(toType, objBase, parentProperty) as MethodBuilder :
                        new VirtualPropertyGetterBuilder(toType, objBase, parentProperty));

                if (typeDescriptor != null)
                    builder.AddInterfaceMethodsExplicitly = typeDescriptor.MethodClashes(parentProperty.GetMethod);
                builder.Build();
                property.SetGetMethod(builder.Method);
            }

            if (parentProperty.SetMethod != null &&
                (parentProperty.SetMethod.IsAbstract || parentProperty.SetMethod.IsVirtual) &&
                !parentProperty.SetMethod.IsPrivate && !parentProperty.SetMethod.IsAssembly)
            {
                var builder = parameterTypes.Any() ?
                    (parentProperty.SetMethod.IsAbstract ?
                        new AbstractIndexSetterBuilder(toType, objBase, parentProperty) as MethodBuilder :
                        new VirtualIndexSetterBuilder(toType, objBase, parentProperty)) :
                    (parentProperty.SetMethod.IsAbstract ?
                        new AbstractPropertySetterBuilder(toType, objBase, parentProperty) as MethodBuilder :
                        new VirtualPropertySetterBuilder(toType, objBase, parentProperty));

                if (typeDescriptor != null)
                    builder.AddInterfaceMethodsExplicitly = typeDescriptor.MethodClashes(parentProperty.SetMethod);
                builder.Build();
                property.SetSetMethod(builder.Method);
            }
        }
    }
}