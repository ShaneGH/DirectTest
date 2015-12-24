using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Dynamox.Mocks;

namespace Dynamox.Compile
{
    public class Compiler
    {
        private readonly ConcurrentDictionary<Type, Type> Built = new ConcurrentDictionary<Type, Type>();
        const string _ObjectBase = "_ObjectBase";
        private const string RootNamespace = "Dynamox.Proxies";
        private const string UnderlyingObject = "__DynamoxTests_BaseObject";    //TODO: ensure unique
        private static readonly BindingFlags AllMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

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

        void CompileAndCache(Type baseType)
        {
            lock (baseType) //TODO: not the best object to lock
            {
                if (!Built.ContainsKey(baseType))
                {
                    Built.TryAdd(baseType, BuildType(baseType));
                }
            }
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

        static int TypeIncrement = new Random().Next(99999);
        Type BuildType(Type baseType)
        {
            var typeDescriptor = TypeOverrideDescriptor.Create(baseType);
            if (typeDescriptor.HasAbstractInternal)
                throw new InvalidOperationException("You cannot mock a class with an internal abstract member");

            var type = Module.DefineType(
                "Dynamox.Proxy." + baseType.Namespace + "." + baseType.Name + "_" + (++TypeIncrement), 
                TypeAttributes.Public | TypeAttributes.Class,
                typeDescriptor.Type,
                typeDescriptor.OverridableInterfaces.Select(i => i.Interface).ToArray());

            var allMembers = baseType.GetMembers(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(m => m.Name).ToArray();

            var fieldName = UnderlyingObject;
            for (var i = 1; allMembers.Contains(fieldName); i++)
            {
                fieldName = UnderlyingObject + i;
            }

            var objBase = type.DefineField(GetFreeMemberName(baseType, fieldName),
                typeof(ObjectBase), FieldAttributes.NotSerialized | FieldAttributes.Private | FieldAttributes.InitOnly);

            foreach (var constructor in typeDescriptor.Type.GetConstructors(AllMembers)
                .Where(c => !c.IsAssembly || c.IsFamilyOrAssembly))
            {
                AddConstructor(type, objBase, constructor, typeDescriptor);
            }

            foreach (var property in typeDescriptor.OverridableProperties)
            {
                AddProperty(type, objBase, property);
            }

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

            foreach (var property in typeDescriptor.OverridableInterfaces.SelectMany(i => i.OverridableProperties))
            {
                //TODO: if property signature is available

                AddProperty(type, objBase, property);
            }

            foreach (var method in typeDescriptor.OverridableInterfaces.SelectMany(i => i.OverridableMethods))
            {
                //TODO: if method signature is available

                var builder = (method.ReturnType == typeof(void) ?
                    (MethodBuilder)new AbstractMethodBuilderNoReturn(type, objBase, method) :
                    new AbstractMethodBuilderWithReturn(type, objBase, method));

                builder.Build();
            }

            return type.CreateType();
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

        public static void AddProperty(TypeBuilder toType, FieldInfo objBase, PropertyInfo parentProperty)
        { 
            if (!parentProperty.IsAbstract() && !parentProperty.IsVirtual())
                throw new InvalidOperationException();  //TODO

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

                builder.Build();
                property.SetGetMethod(builder.Method);
            }

            if (parentProperty.SetMethod != null &&
                (parentProperty.SetMethod.IsAbstract || parentProperty.SetMethod.IsVirtual) &&
                !parentProperty.SetMethod.IsPrivate && !parentProperty.SetMethod.IsAssembly)
            {
                var builder = parameterTypes.Any() ?
                    new IndexSetterBuilder(toType, objBase, parentProperty) as MethodBuilder :
                    new PropertySetterBuilder(toType, objBase, parentProperty);

                builder.Build();
                property.SetSetMethod(builder.Method);
            }
        }

        static void AddConstructor(TypeBuilder toType, FieldInfo objBase, ConstructorInfo constructor, TypeOverrideDescriptor descriptor)
        {
            var args = new[] { typeof(ObjectBase) }
                .Concat(constructor.GetParameters().Select(p => p.ParameterType)).ToArray();
            var con = toType.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, args);

            var body = con.GetILGenerator();

            var ret = body.DefineLabel();

            // Set objectBase
            // arg 0 is "this"
            body.Emit(OpCodes.Ldarg_0);
            // arg 1 is objBase
            body.Emit(OpCodes.Ldarg_1);
            // this.Field = arg1;
            body.Emit(OpCodes.Stfld, objBase);

            // Call base constructor
            body.Emit(OpCodes.Ldarg_0);
            for (var i = 1; i < args.Length; i++)
                body.Emit(OpCodes.Ldarg_S, (short)(i + 1));
            body.Emit(OpCodes.Call, constructor);

            // if (ObjectBase.Settings.SetNonVirtualPropertiesOrFields != true) return;
            body.Emit(OpCodes.Ldarg_1);
            body.Emit(OpCodes.Ldfld, typeof(ObjectBase).GetField("Settings"));
            body.Emit(OpCodes.Call, typeof(DxSettings).GetProperty("SetNonVirtualPropertiesOrFields").GetMethod);
            body.Emit(OpCodes.Ldc_I4_1);
            body.Emit(OpCodes.Ceq);
            body.Emit(OpCodes.Brfalse, ret);

            BuildSetters(body, descriptor);
            body.Emit(OpCodes.Br, ret);

            body.MarkLabel(ret);
            body.Emit(OpCodes.Ret);
        }

        static void BuildSetters(ILGenerator body, TypeOverrideDescriptor forType)
        {
            var hasFieldOrProperty = typeof(ObjectBase).GetMethod("HasFieldOrProperty");
            var getProperty = typeof(ObjectBase).GetMethod("GetProperty");
            Action<string, Type, Action> ifNotHasFieldOrProperty = (n, t, set) =>
            {
                var endFieldSetting = body.DefineLabel();

                // if (!ObjectBase.HasFieldOrProperty<T>("Name")) GO TO: next property
                body.Emit(OpCodes.Ldarg_1);
                body.Emit(OpCodes.Ldstr, n);
                body.Emit(OpCodes.Call, hasFieldOrProperty.MakeGenericMethod(new[] { t }));
                body.Emit(OpCodes.Ldc_I4_0);
                body.Emit(OpCodes.Ceq);
                body.Emit(OpCodes.Brtrue, endFieldSetting);

                // this.Prop = ObjectBase.GetProperty<TProp>("Prop")
                body.Emit(OpCodes.Ldarg_0);
                body.Emit(OpCodes.Ldarg_1);
                body.Emit(OpCodes.Ldstr, n);
                body.Emit(OpCodes.Call, getProperty.MakeGenericMethod(new[] { t }));

                set();

                body.MarkLabel(endFieldSetting);
            };

            foreach (var field in forType.SettableFields)
            {
                ifNotHasFieldOrProperty(field.Name, field.FieldType, () => body.Emit(OpCodes.Stfld, field));
            }

            foreach (var property in forType.SettableProperties)
            {
                ifNotHasFieldOrProperty(property.Name, property.PropertyType, () => body.Emit(OpCodes.Call, property.SetMethod));
            }
        }

        static readonly Regex _global = new Regex(@"^\s*global::\s*");
        static string RemoveNamespace(string test)
        {
            test = _global.Replace(test, "");

            var generic = test.IndexOf("<");
            if (generic == -1)
            {
                if (!test.Contains("."))
                    return test;
                else
                    return test.Substring(test.LastIndexOf(".") + 1);
            }

            var tempIndex = -1;
            var index = 0;
            while ((tempIndex = test.IndexOf(".", tempIndex + 1)) < generic && tempIndex >= 0)
                index = tempIndex;

            return test.Substring(index + 1);
        }
    }
}