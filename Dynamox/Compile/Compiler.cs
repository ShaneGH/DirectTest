using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dynamox.Mocks;
using Microsoft.CSharp;

namespace Dynamox.Compile
{
    public class Compiler
    {
        private readonly Dictionary<Type, Type> Built = new Dictionary<Type, Type>();
        const string _ObjectBase = "_ObjectBase";

        private Compiler()
        {
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

        static readonly ConcurrentDictionary<Type, Tuple<string, IEnumerable<Assembly>>> NameCache = new ConcurrentDictionary<Type, Tuple<string, IEnumerable<Assembly>>>();
        static string GetFullTypeName(Type type, out IEnumerable<Assembly> assemblies, bool includeNamespace = true)
        {
            if (!NameCache.ContainsKey(type))
            {
                var name = type.IsGenericParameter ? type.Name : ("global::" + type.FullName).Replace("+", ".");
                if (type == typeof(void))
                {
                    NameCache.TryAdd(type, new Tuple<string, IEnumerable<Assembly>>(
                        "void",
                        Array.AsReadOnly<Assembly>(type.Assembly == null ? new Assembly[] { } : new[] { type.Assembly })));
                }
                else if (!name.Contains("`"))
                {
                    NameCache.TryAdd(type, new Tuple<string, IEnumerable<Assembly>>(
                        name,
                        Array.AsReadOnly<Assembly>(type.Assembly == null ? new Assembly[] { } : new[] { type.Assembly })));
                }
                else
                {
                    IEnumerable<Assembly> dummy;
                    var ass = new List<Assembly>(type.Assembly == null ? new Assembly[] { } : new[] { type.Assembly });
                    var generics = new List<string>();
                    var output = name.Substring(0, name.IndexOf("`"));
                    foreach (var generic in type.GetGenericArguments())
                    {
                        generics.Add(GetFullTypeName(generic, out dummy));
                        ass.AddRange(dummy);
                    }

                    NameCache.TryAdd(type, new Tuple<string, IEnumerable<Assembly>>(
                        output + "<" + string.Join(", ", generics) + ">",
                        Array.AsReadOnly<Assembly>(ass.Where(a => a != null).Distinct().ToArray())));
                }
            }

            Tuple<string, IEnumerable<Assembly>> result;
            NameCache.TryGetValue(type, out result);
            assemblies = result.Item2;

            return includeNamespace || type.IsGenericParameter ? result.Item1 : RemoveNamespace(result.Item1);
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

        static string GetNameWithGenericHash(Type forType, out IEnumerable<Assembly> assemblies)
        {
            int generic = 0;
            IEnumerable<Assembly> dummy;
            var name = GetFullTypeName(forType, out assemblies, false);
            if ((generic = name.IndexOf("<")) == -1)
                return name;

            return (name.Substring(0, generic) + 
                string.Join("", 
                    forType.GetGenericArguments().Select(a => "_" + GetFullTypeName(a, out dummy).GetHashCode()))).Replace("-", "x");
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
            if (baseType == null)
                baseType = typeof(object);

            if (baseType.IsSealed)
                return new CompilerResult("Cannot mock sealed");  //TODO

            IEnumerable<Assembly> dummy;
            IEnumerable<Assembly> _assemblies;
            var className = GetNameWithGenericHash(baseType, out _assemblies);
            Func<Type, string> getFullTypeName = t =>
            {
                var typeName = GetFullTypeName(t, out dummy);
                _assemblies = _assemblies.Union(dummy);
                return typeName;
            };


            var errors = new CompilerResult();
            var @class = new StringBuilder("namespace Dynamox.Proxy." + baseType.Namespace);//TODO: no namespace
            @class.AppendLine("{");
            @class.AppendLine("public class " + className + ": " + getFullTypeName(baseType));  //TODO: sealed class, generic parameters
            @class.AppendLine("{");

            @class.AppendLine("readonly " + getFullTypeName(typeof(ObjectBase)) + " " + _ObjectBase + ";");

            errors.CompilerErrors.AddRange(ImplementConstructors(baseType, @class, out dummy).CompilerErrors);
            _assemblies = _assemblies.Union(dummy.Where(d => d != null));

            errors.CompilerErrors.AddRange(ImplementProperties(baseType, @class, out dummy).CompilerErrors);
            _assemblies = _assemblies.Union(dummy.Where(d => d != null));

            errors.CompilerErrors.AddRange(ImplementMethods(baseType, @class, out dummy).CompilerErrors);
            _assemblies = _assemblies.Union(dummy.Where(d => d != null));

            @class.AppendLine("}");
            @class.AppendLine("}");

            var ttttttttt = @class.ToString();

            if (errors.CompilerErrors.Any())
                return errors;

            var parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.AddRange(_assemblies.Select(a => a.Location).ToArray());
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;

            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, @class.ToString());

            foreach (CompilerError error in results.Errors)
            {
                errors.CompilerErrors.Add(string.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
            }

            if (errors.CompilerErrors.Any())
                return errors;

            Assembly assembly = results.CompiledAssembly;
            Built[baseType] = assembly.GetType("Dynamox.Proxy." + baseType.Namespace + "." + className);   //TODO: no namespace

            return new CompilerResult();
        }

        static CompilerResult ImplementConstructors(Type baseType, StringBuilder output, out IEnumerable<Assembly> assemblies)
        {
            IEnumerable<Assembly> dummy;
            var _assemblies = new List<Assembly>();
            Func<Type, string> getFullTypeName = t =>
            {
                var typeName = GetFullTypeName(t, out dummy);
                _assemblies.AddRange(dummy);
                return typeName;
            };

            var name = GetNameWithGenericHash(baseType, out dummy);
            _assemblies.AddRange(dummy);
            var constructors = !baseType.IsInterface ?
                baseType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(c => !c.IsPrivate && !c.IsAssembly && !c.IsFamilyOrAssembly)
                    .Select(c => c.GetParameters().Select(p => p.ParameterType)) :
                    new[] { Enumerable.Empty<Type>() };

            if (!constructors.Any())
                throw new InvalidOperationException("No public constructors");  //TODO

            var arg1 = new[]{ typeof(ObjectBase)};
            foreach (var constructor in constructors)
            {
                output.AppendLine("public " +
                    name + //TODO, class name
                    "(" +
                    string.Join(", ", arg1.Concat(constructor).Select((a, i) => getFullTypeName(a) + " arg" + i)) +
                    ")");

                output.AppendLine(": base(" +
                    string.Join(", ", constructor.Select((a, i) => "arg" + (i + 1))) + ")");

                output.AppendLine("{");
                output.AppendLine("this." + _ObjectBase + " = arg0;");
                output.AppendLine("}");
            }

            assemblies = _assemblies.Distinct();
            return new CompilerResult();
        }

        class MethodDescriptor
        {
            public bool IsProtectedInternal { get; set; }
            public bool IsInternal { get; set; }
            public bool IsPrivate { get; set; }
            public bool IsProtected { get; set; }
            public bool IsOverride { get; set; }
            public bool IsAbstract { get; set; }
            public string InterfaceName { get; set; }
        }

        static bool IsFinalize(MethodInfo method) 
        {
            return method.Name == "Finalize" && method.ReturnType == typeof(void) && !method.GetParameters().Any();
        }

        static MethodDescriptor GetMethod(MethodInfo method, Func<Type, string> getFullTypeName, bool suppressInterfaceName)
        {
            return new MethodDescriptor
            {
                IsProtectedInternal = method.IsFamilyOrAssembly,
                IsInternal = method.IsAssembly,
                IsPrivate = method.IsPrivate,
                IsProtected = method.IsFamily,
                IsOverride = !method.DeclaringType.IsInterface,
                InterfaceName = method.DeclaringType.IsInterface && !suppressInterfaceName ? (getFullTypeName(method.DeclaringType) + ".") : string.Empty,
                IsAbstract = method.DeclaringType.IsInterface || method.IsAbstract
            };
        }

        static CompilerResult ImplementProperties(Type baseType, StringBuilder output, out IEnumerable<Assembly> assemblies)
        {
            IEnumerable<Assembly> dummy;
            var _assemblies = new List<Assembly>();
            Func<Type, string> getFullTypeName = t =>
            {
                var typeName = GetFullTypeName(t, out dummy);
                _assemblies.AddRange(dummy);
                return typeName;
            };

            var result = new CompilerResult();

            var required = baseType.AllClassesAndInterfaces();
            var properties = required.SelectMany(c =>
                c.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(p => p.GetAccessors(true).Any(a => !a.IsPrivate && !a.IsAssembly && (c.IsInterface || a.IsVirtual || a.IsAbstract))))
                    .Distinct()
                    .Select(p => new
                    {
                        name = p.Name,
                        getter = p.GetMethod == null ? null : GetMethod(p.GetMethod, getFullTypeName, p.DeclaringType == baseType),
                        setter = p.SetMethod == null ? null : GetMethod(p.SetMethod, getFullTypeName, p.DeclaringType == baseType),
                        propertyType = getFullTypeName(p.PropertyType),
                        isOverride = !p.DeclaringType.IsInterface,
                        declaringType = p.DeclaringType
                    });

            foreach (var property in properties)
            {
                if (property.getter != null && property.getter.IsAbstract && property.getter.IsInternal)
                {
                    result.CompilerErrors.Add("Cannot implement class with abstract internal member: get_" + property.name);
                    continue;
                }

                if (property.setter != null && property.setter.IsAbstract && property.setter.IsInternal)
                {
                    result.CompilerErrors.Add("Cannot implement class with abstract internal member: set_" + property.name);
                    continue;
                }

                bool isProtected = (property.getter == null || property.getter.IsProtected || property.getter.IsProtectedInternal) &&
                    (property.setter == null || property.setter.IsProtected || property.setter.IsProtectedInternal);

                string interfaceName = property.getter != null && !string.IsNullOrEmpty(property.getter.InterfaceName) ?
                    property.getter.InterfaceName :
                    (property.setter != null && !string.IsNullOrEmpty(property.setter.InterfaceName) ?
                        property.setter.InterfaceName : "");

                output.AppendLine(
                    (isProtected ? "protected " : property.isOverride || property.declaringType == baseType ? "public " : string.Empty) +
                    (property.isOverride ? "override " : "") +
                    property.propertyType + " " +
                    interfaceName + property.name);

                output.AppendLine("{");

                Func<MethodDescriptor, bool> ok = a =>
                   a != null && !a.IsPrivate && !a.IsInternal;

                if (ok(property.getter))
                {
                    output.AppendLine((!isProtected && property.getter.IsProtected ? "protected " : "") + "get");
                    output.AppendLine("{");
                    if (property.getter.IsAbstract)
                    {
                        output.AppendLine("return this." + _ObjectBase + ".GetProperty<" + property.propertyType + ">(\"" + property.name + "\");");
                    }
                    else
                    {
                        output.AppendLine(property.propertyType + " val;");
                        output.AppendLine("if (this." + _ObjectBase + ".TryGetProperty<" + property.propertyType + ">(\"" + property.name + "\", out val))");
                        output.AppendLine("return val;");
                        output.AppendLine();
                        output.AppendLine("return base." + property.name + ";");
                    }

                    output.AppendLine("}");
                }

                if (ok(property.setter))
                {
                    output.AppendLine((!isProtected && property.setter.IsProtected ? "protected " : "") + "set");
                    output.AppendLine("{");
                    if (property.setter.IsAbstract)
                    {
                        output.AppendLine("this." + _ObjectBase + ".SetProperty(\"" + property.name + "\", value);");
                    }
                    else
                    {
                        output.AppendLine(property.propertyType + " val;");
                        output.AppendLine("if (this." + _ObjectBase + ".TryGetProperty<" + property.propertyType + ">(\"" + property.name + "\", out val))");
                        output.AppendLine("this." + _ObjectBase + ".SetProperty(\"" + property.name + "\", value);");
                        output.AppendLine("else");
                        output.AppendLine("base." + property.name + " = value;");
                    }
                    
                    output.AppendLine("}");
                }

                output.AppendLine("}");
            }

            assemblies = _assemblies.AsReadOnly();
            return result;
        }

        static CompilerResult ImplementMethods(Type baseType, StringBuilder output, out IEnumerable<Assembly> assemblies)
        {
            IEnumerable<Assembly> dummy;
            var _assemblies = new List<Assembly>();
            Func<Type, string> getFullTypeName = t =>
            {
                var typeName = GetFullTypeName(t, out dummy);
                _assemblies.AddRange(dummy);
                return typeName;
            };

            var required = baseType.AllClassesAndInterfaces();
            var propertyAccessors = new HashSet<MethodInfo>(required.SelectMany(
                r => r.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .SelectMany(p => new[] { p.GetMethod, p.SetMethod }).Where(m => m != null)));

            var methods = required.SelectMany(c => c.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => !propertyAccessors.Contains(m) && 
                    !IsFinalize(m) &&
                    (c.IsInterface || m.IsAbstract || m.IsVirtual)))
                .Distinct(MethodInfoComparer.Instance)
                .Select(m => new
                {
                    name = m.Name,
                    method = GetMethod(m, getFullTypeName, m.DeclaringType == baseType),
                    declaringType = m.DeclaringType,
                    returnType = getFullTypeName(m.ReturnType),
                    isOverride = !m.DeclaringType.IsInterface,
                    parameters = m.GetParameters(),
                    generics = m.GetGenericArguments()
                })
                .ToArray();

            var result = new CompilerResult();
            var kvp = getFullTypeName(typeof(KeyValuePair<Type, object>));
            foreach (var method in methods)
            {
                if (method.method.IsInternal)
                {
                    if (method.method.IsAbstract)
                        result.CompilerErrors.Add("Cannot implement class with abstract internal member: " + method.name);
                    continue;
                }

                output.Append(
                    (method.method.IsProtected || method.method.IsProtectedInternal ? "protected " : 
                        method.method.IsOverride || method.declaringType == baseType ? "public " : string.Empty) +
                    (method.method.IsOverride ? "override " : "") +
                    method.returnType + " " +
                    method.method.InterfaceName  + method.name);

                if (method.generics.Any())
                    output.Append("<" + string.Join(", ", method.generics.Select(getFullTypeName)) + ">");

                output.AppendLine("(" + string.Join(", ", method.parameters.Select((p, i) => getFullTypeName(p.ParameterType) + " arg" + i)) + ")");
                output.AppendLine("{");

                if (method.method.IsAbstract || !string.IsNullOrWhiteSpace(method.method.InterfaceName))
                {
                    output.AppendLine((method.returnType == "void" ? "" : "return ") +
                        "this." + _ObjectBase + ".Invoke" +
                        (method.returnType == "void" ? "" : ("<" + method.returnType + ">")) +
                        "(\"" + method.name + "\"" +
                        (method.generics.Any() ? ", new [] {" + string.Join(", ", method.generics.Select(t => "typeof(" + getFullTypeName(t) + ")" )) + "}" : "") +
                        ", new " + kvp + "[]" + " {" +
                        string.Join(", ", method.parameters.Select((p, i) => "new " + kvp + "(typeof(" + getFullTypeName(p.ParameterType) + "), arg" + i + ")")) +
                        "});");
                }
                else
                {
                    // Type output;
                    if (method.returnType != "void")
                        output.AppendLine(method.returnType + " output;");

                    // if (!this._ObjectBase.TryInvoke("name", args, out ouptup))
                    output.AppendLine("if (!this." + _ObjectBase + ".TryInvoke" +
                        (method.returnType == "void" ? "" : ("<" + method.returnType + ">")) +
                        "(\"" + method.name + "\"" +
                        (method.generics.Any() ? ", new [] {" + string.Join(", ", method.generics.Select(t => "typeof(" + getFullTypeName(t) + ")")) + "}" : "") +
                        ", new " + kvp + "[]" + " {" +
                        string.Join(", ", method.parameters.Select((p, i) => "new " + kvp + "(typeof(" + getFullTypeName(p.ParameterType) + "), arg" + i + ")")) + "}" +
                        (method.returnType != "void" ? ", out output" : "") +
                        "))");

                    // output = base.name(args);
                    output.AppendLine("\t" + (method.returnType == "void" ? "" : "output = ") +
                        "base." + method.name + "(" + string.Join(", ", method.parameters.Select((p, i) => "arg" + i)) + ");");

                    // return output
                    if (method.returnType != "void")
                        output.AppendLine("return output;");
                }

                output.AppendLine("}");
            }

            assemblies = _assemblies.AsReadOnly();
            return new CompilerResult();
        }


        static readonly Compiler Instance = new Compiler();
        public static Type Compile(Type baseType) 
        {
            return Instance._Compile(baseType);
        }
    }
}