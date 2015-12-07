using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dynamox.Mocks;

namespace Dynamox.Compile
{//TypeBuilder
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

        private Compiler2()
        {
            Assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(RootNamespace), AssemblyBuilderAccess.Run);
        }

        Type _Compile(Type baseType)
        {
            if (!Built.ContainsKey(baseType))
            {
                var result = CompileAndCache(baseType);
                if (result.CompilerErrors.Any())
                    throw new InvalidOperationException(string.Join(@"\n", result.CompilerErrors));  //TODO
            }

            return Built[baseType];
        }

        class CompilerResult
        {
            public readonly List<string> CompilerErrors;

            public CompilerResult(params string[] errors)
            {
                CompilerErrors = new List<string>(errors ?? new string[0]);
            }
        }

        CompilerResult CompileAndCache(Type baseType)
        {
            if (!Built.ContainsKey(baseType))
            {
                Built.TryAdd(baseType, BuildType(baseType));
            }

            return new CompilerResult();
        }

        MethodInfo GetMethodOverride(Type childType, MethodInfo parentMethod)
        {
            return childType.AllClassesAndInterfaces()
                    .SelectMany(c => c.GetMethods(AllMembers))
                    .Where(m => m.Name == parentMethod.Name && m != parentMethod && m.DeclaringType != parentMethod.DeclaringType &&
                        parentMethod.DeclaringType.IsAssignableFrom(m.DeclaringType) && m.ReturnType == parentMethod.ReturnType &&
                         AreEqual(m.GetParameters().Select(p => p.ParameterType), parentMethod.GetParameters().Select(p => p.ParameterType)))
                    .FirstOrDefault();
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
            if (baseType.AllClassesAndInterfaces()
                    .SelectMany(c => c.GetMethods(AllMembers))
                    .Any(m => m.IsAbstract && m.IsAssembly && GetMethodOverride(baseType, m) == null))
                throw new InvalidOperationException("Cannot mock a class with an internal abstract member");

            var interfaces = baseType.IsInterface ? new []{baseType} : new Type[0];
            if (interfaces.Any())
                baseType = typeof(object);

            var module = Assembly.DefineDynamicModule(RootNamespace);
            var type = module.DefineType(
                "Dynamox.Proxy." + baseType.Namespace + "." + GetFullTypeName(baseType, false), 
                TypeAttributes.Public | TypeAttributes.Class, 
                baseType.IsInterface ? typeof(object) : baseType,
                interfaces);

            var objBase = type.DefineField(GetFreeMemberName(baseType, UnderlyingObject), typeof(ObjectBase), FieldAttributes.NotSerialized | FieldAttributes.Private | FieldAttributes.InitOnly);

            foreach (var constructor in baseType.GetConstructors(AllMembers)
                .Where(c => !c.IsAssembly || c.IsFamilyOrAssembly))
            {
                AddConstructor(type, objBase, constructor);
            }

            foreach (var property in baseType.GetProperties(AllMembers)
                .Where(p => p.GetAccessors(true).Any(a => !a.IsAssembly && !a.IsPrivate)))
            {
                AddProperty(type, objBase, property);
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

            Func<MethodInfo, MethodAttributes> getAttrs = a =>
                {
                    var _base = MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;
                    if (a.IsPublic)
                        _base = _base | MethodAttributes.Public;
                    else if (a.IsFamilyOrAssembly)
                        _base = _base | MethodAttributes.FamORAssem;
                    else if (a.IsFamily)
                        _base = _base | MethodAttributes.Family;
                    else if (a.IsAssembly)
                        _base = _base | MethodAttributes.Assembly;
                    else if (a.IsFamilyAndAssembly)
                        _base = _base | MethodAttributes.FamANDAssem;
                    else if (a.IsPrivate)
                        _base = _base | MethodAttributes.Private;

                    return _base;
                };

            if (parentProperty.GetMethod != null)
            {
                var getAttr = getAttrs(parentProperty.GetMethod);

                // Define the "get" accessor method for CustomerName.
                var getter = toType.DefineMethod("get_" + parentProperty.Name, getAttr, parentProperty.PropertyType, Type.EmptyTypes);
                //TODO: DefineMethodOverride
                var body = getter.GetILGenerator();

                body.Emit(OpCodes.Ldarg_0);
                body.Emit(OpCodes.Ldfld, objBase);

                body.Emit(OpCodes.Ldstr, parentProperty.Name);
                if (parentProperty.IsAbstract())
                    body.Emit(OpCodes.Call, typeof(ObjectBase).GetMethod("GetProperty").MakeGenericMethod(parentProperty.PropertyType));
                else
                    throw new NotImplementedException();

                body.Emit(OpCodes.Ret);

                property.SetGetMethod(getter);
            }

            if (parentProperty.SetMethod != null)
            {
                var setAttr = getAttrs(parentProperty.SetMethod);

                // Define the "get" accessor method for CustomerName.
                var setter = toType.DefineMethod("set_" + parentProperty.Name, setAttr, null, new []{ parentProperty.PropertyType });
                var body = setter.GetILGenerator();

                body.Emit(OpCodes.Ldarg_0);
                body.Emit(OpCodes.Ldfld, objBase);

                body.Emit(OpCodes.Ldstr, parentProperty.Name);
                body.Emit(OpCodes.Ldarg_1);
                if (parentProperty.IsAbstract())
                    body.Emit(OpCodes.Call, typeof(ObjectBase).GetMethod("SetProperty"));
                else
                    throw new NotImplementedException();

                body.Emit(OpCodes.Ret);

                property.SetSetMethod(setter);
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