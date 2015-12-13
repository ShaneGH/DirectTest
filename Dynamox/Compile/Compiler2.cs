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
    public class Compiler2
    {
        private readonly ConcurrentDictionary<Type, Type> Built = new ConcurrentDictionary<Type, Type>();
        const string _ObjectBase = "_ObjectBase";
        private const string RootNamespace = "Dynamox.Proxies";
        private const string UnderlyingObject = "__DynamoxTests_BaseObject";    //TODO: ensure unique
        private static readonly BindingFlags AllMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        static readonly Compiler2 Instance = new Compiler2();
        public static Type Compile(Type baseType) 
        {
            return Instance._Compile(baseType);
        }

        readonly AssemblyBuilder Assembly;
        readonly ModuleBuilder Module;

        private Compiler2()
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

        Type BuildType(Type baseType)
        {
            var typeDescriptor = new TypeOverrideDescriptor(baseType);
            if (typeDescriptor.HasAbstractInternal)
throw new InvalidOperationException("Cannot mock a class with an internal abstract member");
var type = Module.DefineType(
"Dynamox.Proxy." + baseType.Namespace + "." + GetFullTypeName(baseType, false), 
TypeAttributes.Public | TypeAttributes.Class,
baseType.IsInterface ? typeof(object) : baseType,
typeDescriptor.OverridableInterfaces.Select(i => i.Interface).ToArray());

            var objBase = type.DefineField(GetFreeMemberName(baseType, UnderlyingObject),
                typeof(ObjectBase), FieldAttributes.NotSerialized | FieldAttributes.Private | FieldAttributes.InitOnly);

            foreach (var constructor in baseType.GetConstructors(AllMembers)
                .Where(c => !c.IsAssembly || c.IsFamilyOrAssembly))
            {
                AddConstructor(type, objBase, constructor);
            }

            //TODO: interface properties
            foreach (var property in typeDescriptor.OverridableProperties)
            {
                AddProperty(type, objBase, property);
            }

            //TODO: interface methods
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

            var property = toType.DefineProperty(parentProperty.Name, PropertyAttributes.None, parentProperty.PropertyType, null);

            if (parentProperty.GetMethod != null && 
                (parentProperty.GetMethod.IsAbstract || parentProperty.GetMethod.IsVirtual) && 
                !parentProperty.GetMethod.IsPrivate && !parentProperty.GetMethod.IsAssembly)
            {
                var builder = parentProperty.GetMethod.IsAbstract ?
                    (MethodBuilder)new AbstractPropertyGetterBuilder(toType, objBase, parentProperty) :
                    (MethodBuilder)new VirtualPropertyGetterBuilder(toType, objBase, parentProperty);

                builder.Build();
                property.SetGetMethod(builder.Method);
            }

            if (parentProperty.SetMethod != null &&
                (parentProperty.SetMethod.IsAbstract || parentProperty.SetMethod.IsVirtual) &&
                !parentProperty.SetMethod.IsPrivate && !parentProperty.SetMethod.IsAssembly)
            {
                var builder = new PropertySetterBuilder(toType, objBase, parentProperty);

                builder.Build();
                property.SetSetMethod(builder.Method);
            }
        }

        public static void AddConstructor(TypeBuilder toType, FieldInfo objBase, ConstructorInfo constructor)
        {
            var args = new[] { typeof(ObjectBase) }
                .Concat(constructor.GetParameters().Select(p => p.ParameterType)).ToArray();
            var con = toType.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, args);

            var body = con.GetILGenerator();

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

            body.Emit(OpCodes.Ret);
        }

        static readonly ConcurrentDictionary<Type, string> NameCache = new ConcurrentDictionary<Type, string>(new Dictionary<Type, string> { { typeof(void), "void" } });
        static string GetFullTypeName(Type type, bool includeNamespace = true)
        {
            if (!NameCache.ContainsKey(type))
            {
                var name = type.IsGenericParameter ? type.Name : ("global::" + type.FullName).Replace("+", ".");
                if (!name.Contains("`"))
                {
                    NameCache.TryAdd(type, name);
                }
                else
                {
                    var generics = new List<string>();
                    var output = name.Substring(0, name.IndexOf("`"));
                    foreach (var generic in type.GetGenericArguments())
                    {
                        generics.Add(GetFullTypeName(generic));
                    }

                    NameCache.TryAdd(type, output + "<" + string.Join(", ", generics) + ">");
                }
            }

            string result;
            NameCache.TryGetValue(type, out result);

            return includeNamespace || type.IsGenericParameter ? result : RemoveNamespace(result);
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