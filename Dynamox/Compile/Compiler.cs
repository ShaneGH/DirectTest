using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading;
using Dynamox.Mocks;

namespace Dynamox.Compile
{
    public class Compiler
    { 
        private readonly ConcurrentDictionary<Type, Type> Built = new ConcurrentDictionary<Type, Type>();
        const string _ObjectBase = "_ObjectBase";
        private const string RootNamespace = "Dynamox.Proxies";
        private const string UnderlyingObject = "__DynamoxTests_BaseObject";
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
                throw new InvalidOperationException("", compileException);  //TODE
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
                throw new InvalidOperationException("You cannot mock a nested private class or interface"); //TODE

            if (baseType.IsNestedAssembly)
                throw new InvalidOperationException("You cannot mock a nested internal class or interface"); //TODE
                        
            var typeDescriptor = TypeOverrideDescriptor.Create(baseType);
            if (typeDescriptor.HasAbstractInternal)
                throw new InvalidOperationException("You cannot mock a class with an internal abstract member"); //TODE
            
            // define type
            var type = Module.DefineType(
                "Dynamox.Proxy." + baseType.Namespace + "." + baseType.Name + "_" + (++TypeIncrement), 
                TypeAttributes.Public | TypeAttributes.Class,
                typeDescriptor.Type,
                typeDescriptor.OverridableInterfaces.Select(i => i.Interface).ToArray());

            var allMembers = baseType.GetMembers(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(m => m.Name).ToArray();

            // define ObjectBase field
            var objBase = type.DefineField(GetFreeMemberName(baseType, UnderlyingObject),
                typeof(ObjectBase), FieldAttributes.NotSerialized | FieldAttributes.Private | FieldAttributes.InitOnly);

            // add constructors
            foreach (var constructor in typeDescriptor.Type.GetConstructors(AllMembers)
                .Where(c => !c.IsAssembly || c.IsFamilyOrAssembly))
            {
                AddConstructor(type, objBase, constructor, typeDescriptor);
            }

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

            // add interface properties
            foreach (var property in typeDescriptor.OverridableInterfaces.SelectMany(i => i.OverridableProperties))
            {
                //TODO: if property signature is available

                AddProperty(type, objBase, property);
            }

            // add interface methods
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
                    (parentProperty.SetMethod.IsAbstract ?
                        new AbstractIndexSetterBuilder(toType, objBase, parentProperty) as MethodBuilder :
                        new VirtualIndexSetterBuilder(toType, objBase, parentProperty)) :
                    (parentProperty.SetMethod.IsAbstract ?
                        new AbstractPropertySetterBuilder(toType, objBase, parentProperty) as MethodBuilder :
                        new VirtualPropertySetterBuilder(toType, objBase, parentProperty));

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

        static readonly MethodInfo HasMockedFieldOrProperty = typeof(ObjectBase).GetMethod("HasMockedFieldOrProperty");
        static readonly MethodInfo GetProperty = typeof(ObjectBase).GetMethod("GetProperty");
        static void AddSetter(ILGenerator body, string propertyName, Type propertyType, Action doSet)
        {
            var types = new[] { propertyType };
            var endFieldSetting = body.DefineLabel();

            // if (!ObjectBase.HasFieldOrProperty<T>("Name")) GO TO: next property
            body.Emit(OpCodes.Ldarg_1);
            body.Emit(OpCodes.Ldstr, propertyName);
            body.Emit(OpCodes.Call, HasMockedFieldOrProperty.MakeGenericMethod(types));
            body.Emit(OpCodes.Ldc_I4_0);
            body.Emit(OpCodes.Ceq);
            body.Emit(OpCodes.Brtrue, endFieldSetting);

            // this.Prop = ObjectBase.GetProperty<TProp>("Prop")
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldarg_1);
            body.Emit(OpCodes.Ldstr, propertyName);
            body.Emit(OpCodes.Call, GetProperty.MakeGenericMethod(types));

            doSet();

            body.MarkLabel(endFieldSetting);
        }

        protected static readonly FieldInfo Arg = typeof(MethodArg).GetField("Arg");
        protected static readonly MethodInfo ElementAt = typeof(Enumerable).GetMethod("ElementAt", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(new[] { typeof(MethodArg) });
        protected static readonly MethodInfo Current = typeof(IEnumerator<IEnumerable<MethodArg>>).GetProperty("Current").GetMethod;
        protected static readonly MethodInfo GetEnumerator = typeof(IEnumerable<IEnumerable<MethodArg>>).GetMethod("GetEnumerator");
        protected static readonly MethodInfo GetIndex = typeof(ObjectBase).GetMethod("GetIndex");
        protected static readonly MethodInfo MoveNext = typeof(IEnumerator).GetMethod("MoveNext");
        protected static readonly MethodInfo GetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle");
        protected static readonly MethodInfo GetMockedIndexKeys = typeof(ObjectBase).GetMethod("GetMockedIndexKeys");
        static void AddIndexSetter(ILGenerator body, MethodInfo setMethod)
        {
            var allParamaters = setMethod.GetParameters().Select(p => p.ParameterType);
            var indexTypes = allParamaters.Take(allParamaters.Count() - 1).ToArray();
            var propertyType = allParamaters.Last();

            var types = new[] { propertyType };

            // var indexes = new Type[i];
            var indexes = body.DeclareLocal(typeof(Type[]));
            body.Emit(OpCodes.Ldc_I4, indexTypes.Count());
            body.Emit(OpCodes.Newarr, typeof(Type));
            body.Emit(OpCodes.Stloc, indexes);

            for (var i = 0; i < indexTypes.Length; i++)
            {
                // indexes[i] = typeof(TKey);
                body.Emit(OpCodes.Ldloc, indexes);
                body.Emit(OpCodes.Ldc_I4, i);
                body.Emit(OpCodes.Ldtoken, indexTypes[i]);
                body.Emit(OpCodes.Call, GetTypeFromHandle);
                body.Emit(OpCodes.Stelem_Ref);
            }

            // var result = ObjectBase.GetMockedIndexKeys<TProperty>(indexes)
            var result = body.DeclareLocal(typeof(IEnumerable<IEnumerable<MethodArg>>));
            body.Emit(OpCodes.Ldarg_1);
            body.Emit(OpCodes.Ldloc, indexes);
            body.Emit(OpCodes.Call, GetMockedIndexKeys.MakeGenericMethod(types));
            body.Emit(OpCodes.Stloc, result);

            // var enumerable = result.GetEnumerator();
            var enumerable = body.DeclareLocal(typeof(IEnumerator<IEnumerable<MethodArg>>));
            body.Emit(OpCodes.Ldloc, result);
            body.Emit(OpCodes.Callvirt, GetEnumerator);
            body.Emit(OpCodes.Stloc, enumerable);

            var value = body.DeclareLocal(propertyType);
            var keys = body.DeclareLocal(typeof(IEnumerable<MethodArg>));
            var startLoop = body.DefineLabel();
            var endLoop = body.DefineLabel();

            body.MarkLabel(startLoop);

            // if (!enumerable.MoveNext())  GO TO END
            body.Emit(OpCodes.Ldloc, enumerable);
            body.Emit(OpCodes.Callvirt, MoveNext);
            body.Emit(OpCodes.Ldc_I4_0);
            body.Emit(OpCodes.Ceq);
            body.Emit(OpCodes.Brtrue, endLoop);

            // keys = enumerable.Current
            body.Emit(OpCodes.Ldloc, enumerable);
            body.Emit(OpCodes.Callvirt, Current);
            body.Emit(OpCodes.Stloc, keys);

            // value = ObjectBase.GetIndex<TIndexed>(IEnumerable<MethodArg> indexValues)
            body.Emit(OpCodes.Ldarg_1);
            body.Emit(OpCodes.Ldloc, keys);
            body.Emit(OpCodes.Call, GetIndex.MakeGenericMethod(new[] { propertyType }));
            body.Emit(OpCodes.Stloc, value);
            
            var vars = indexTypes.Select(it => body.DeclareLocal(it)).ToArray();

            for (var i = 0; i < vars.Length; i++)
            {
                //var0 = (T)keys.ElementAt(0).Arg;
                body.Emit(OpCodes.Ldloc, keys);
                body.Emit(OpCodes.Ldc_I4, i);
                body.Emit(OpCodes.Call, ElementAt);
                body.Emit(OpCodes.Ldfld, Arg);
                if (indexTypes[i].IsValueType)
                    body.Emit(OpCodes.Unbox_Any, indexTypes[i]);
                else
                    body.Emit(OpCodes.Castclass, indexTypes[i]);

                body.Emit(OpCodes.Stloc, vars[i]);
            }

            body.Emit(OpCodes.Ldarg_0);
            for (var i = 0; i < vars.Length; i++)
            {
                body.Emit(OpCodes.Ldloc, vars[i]);
            }

            body.Emit(OpCodes.Ldloc, value);
            body.Emit(OpCodes.Call, setMethod);
            
            // GO TO START
            body.Emit(OpCodes.Br, startLoop);

            body.MarkLabel(endLoop);
        }

        static void BuildSetters(ILGenerator body, TypeOverrideDescriptor forType)
        {
            foreach (var field in forType.SettableFields)
            {
                AddSetter(body, field.Name, field.FieldType, () => body.Emit(OpCodes.Stfld, field));
            }

            foreach (var property in forType.SettableProperties.Where(p => !p.GetIndexParameters().Any()))
            {
                AddSetter(body, property.Name, property.PropertyType, () => body.Emit(OpCodes.Call, property.SetMethod));
            }

            foreach (var property in forType.SettableProperties.Where(p => p.GetIndexParameters().Any()))
            {
                AddIndexSetter(body, property.SetMethod);
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