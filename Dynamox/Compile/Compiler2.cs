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
            var interfaces = baseType.IsInterface ? new []{baseType} : new Type[0];
            if (interfaces.Any())
                baseType = typeof(object);

            var module = Assembly.DefineDynamicModule(RootNamespace);
            var type = module.DefineType(
                "Dynamox.Proxy." + GetFullTypeName(baseType), 
                TypeAttributes.Public | TypeAttributes.Class, 
                baseType.IsInterface ? typeof(object) : baseType,
                interfaces);

            foreach (var constructor in 
                baseType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic)
                .Where(c => !c.IsAssembly || c.IsFamilyOrAssembly))
            {
                AddConstructor(type, constructor);
            }

            var module = new ModuleBuilder();
            //TypeBuilder f = new TypeBuilder();

            return null;
        }

        public static void AddConstructor(TypeBuilder toType, ConstructorInfo constructor)
        {
            var con = toType.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis,
                new [] {typeof(ObjectBase)}.Concat(constructor.GetParameters().Select(p => p.ParameterType)).ToArray());

            var body = con.GetILGenerator();

            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldarg_1);
            body.Emit(OpCodes.Stfld, myGreetingField);
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