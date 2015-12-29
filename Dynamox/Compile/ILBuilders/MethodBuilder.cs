using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;

namespace Dynamox.Compile.ILBuilders
{
    /// <summary>
    /// Base class to build a method
    /// </summary>
    public abstract class MethodBuilder : NewBlockILBuilder
    {
        protected static readonly FieldInfo MethodArg_Arg = typeof(MethodArg).GetField("Arg");

        protected readonly MethodInfo ParentMethod;

        protected GenericTypeParameterBuilder[] Generics { get; private set; }
        protected readonly ParameterInfo[] Parameters;
        protected readonly Type[] ParameterTypes;
        public System.Reflection.Emit.MethodBuilder Method { get; private set; }
        protected ILGenerator Body { get; private set; }

        public MethodBuilder(TypeBuilder toType, FieldInfo objBase, MethodInfo parentMethod)
            : base(toType, objBase)
        {
            ParentMethod = parentMethod;

            Parameters = ParentMethod.GetParameters();
            ParameterTypes = Parameters.Select(p => p.ParameterType).ToArray();
        }

        protected virtual MethodAttributes GetAttrs(MethodInfo forMethod)
        {
            var _base = MethodAttributes.HideBySig | MethodAttributes.Virtual;
            if (forMethod.DeclaringType.IsInterface)
                return _base | MethodAttributes.Private | MethodAttributes.NewSlot | MethodAttributes.Final;

            _base = _base | MethodAttributes.HideBySig;
            var access = GetAccessAttr(forMethod);
            if (access.HasValue)
                _base = _base | access.Value;

            return _base;
        }

        LocalBuilder CreateArray(Type arrayType, int length)
        {
            var array = Body.DeclareLocal(arrayType.MakeArrayType());
            Body.Emit(OpCodes.Ldc_I4, length);
            Body.Emit(OpCodes.Newarr, arrayType);
            Body.Emit(OpCodes.Stloc, array);

            return array;
        }

        MethodInfo _ConvertFromRefType = typeof(MethodBuilder).GetMethod("ConvertFromRefType", BindingFlags.Public | BindingFlags.Static);
        void AddParameterToInputArray(int index, LocalBuilder array)
        {
            var paramType = ParameterTypes[index].IsByRef ?
                    ParameterTypes[index].GetElementType() :
                    ParameterTypes[index];

            var mockType = typeof(MethodArg<>).MakeGenericType(paramType);

            Body.Emit(OpCodes.Ldloc, array);
            Body.Emit(OpCodes.Ldc_I4, index);

            if (Parameters[index].IsOut)
            {
                Body.Emit(OpCodes.Ldstr, ParentMethod.GetParameters()[index].Name);
                Body.Emit(OpCodes.Newobj, mockType.GetConstructor(new[] { typeof(string) }));
            }
            else
            {
                Body.Emit(OpCodes.Ldarg, index + 1);
                if (ParameterTypes[index].IsByRef)
                {
                    // TODO: do conversion in IL
                    Body.Emit(OpCodes.Call, _ConvertFromRefType.MakeGenericMethod(new[] { paramType }));
                }

                Body.Emit(OpCodes.Ldstr, ParentMethod.GetParameters()[index].Name);
                Body.Emit(OpCodes.Newobj, mockType.GetConstructor(new[] { paramType, typeof(string) }));
            }

            // = new MethodArg<T>(value);
            Body.Emit(OpCodes.Stelem_Ref);
        }

        public static T ConvertFromRefType<T>(ref T val)
        {
            return val;
        }

        protected abstract LocalBuilder CallMockedMethod(LocalBuilder generics, LocalBuilder args, LocalBuilder methodOut);

        void AssignOutParameter(int argumentIndex, LocalBuilder args)
        {
            var paramType = ParameterTypes[argumentIndex].GetElementType();

            // arg_i
            Body.Emit(OpCodes.Ldarg, argumentIndex + 1);

            // args[i].Arg
            Body.LoadArrayElement(args, argumentIndex);
            Body.Emit(OpCodes.Ldfld, MethodArg_Arg);

            // arg_i = (T)args[i].Arg;
            if (paramType.IsGenericParameter || paramType.IsValueType)
            {
                Body.Emit(OpCodes.Unbox_Any, paramType);
                Body.Emit(OpCodes.Stobj, paramType);
            }
            else
            {
                if (ParameterTypes[argumentIndex] != typeof(object))
                    Body.Emit(OpCodes.Castclass, paramType);
                Body.Emit(OpCodes.Stind_Ref);   //TODO: if it is a ref parameter, does setting the value in the method change the output (value and ref types)
            }
        }

        public void OnSuccess(LocalBuilder args, Label onReturn)
        {
            Body.Emit(OpCodes.Nop);
            for (var i = 0; i < Parameters.Length; i++)
            {
                if (!ParameterTypes[i].IsByRef) //TODO: test
                    continue;

                AssignOutParameter(i, args);
            }

            Body.Emit(OpCodes.Br, onReturn);
        }

        public void OnFailure(LocalBuilder outputValue, Label onReturn)
        {
            // outputValue = base.Method(args...);
            Body.Emit(OpCodes.Ldarg_0);
            for (var i = 0; i < Parameters.Length; i++)
                Body.Emit(OpCodes.Ldarg, i + 1);

            Body.Emit(OpCodes.Call, ParentMethod);

            if (ParentMethod.ReturnType != typeof(void))
                Body.Emit(OpCodes.Stloc, outputValue);

            Body.Emit(OpCodes.Br, onReturn);
        }

        public void OnReturn(LocalBuilder outputValue)
        {
            if (ParentMethod.ReturnType != typeof(void))
            {
                var returnValue = Body.DeclareLocal(ParentMethod.ReturnType);

                // var returnValue = outputValue;
                Body.Emit(OpCodes.Ldloc, outputValue);
                Body.Emit(OpCodes.Stloc, returnValue);

                // return returnValue;
                Body.Emit(OpCodes.Ldloc, returnValue);
            }

            Body.Emit(OpCodes.Ret);
        }

        protected override sealed void _Build()
        {
            if (!ParentMethod.IsAbstract && !ParentMethod.IsVirtual)
                throw new InvalidOperationException();  //TODE

            var name = ParentMethod.DeclaringType.IsInterface ? ParentMethod.DeclaringType.Name + "." + ParentMethod.Name : ParentMethod.Name;
            Method = TypeBuilder.DefineMethod(name, GetAttrs(ParentMethod), ParentMethod.ReturnType, ParameterTypes);
            Body = Method.GetILGenerator();

            if (!ParentMethod.ContainsGenericParameters)
            {
                Generics = new GenericTypeParameterBuilder[0];
            }
            else
            {
                Generics = Method.DefineGenericParameters(ParentMethod.GetGenericArguments().Select((g, i) => "T" + 1).ToArray());
            }

            var onFailure = Body.DefineLabel();
            var onReturn = Body.DefineLabel();

            // define types
            var methodOut = ParentMethod.ReturnType != typeof(void) ? Body.DeclareLocal(ParentMethod.ReturnType) : null;
            var argsForCall = Body.DeclareLocal(typeof(MethodArg[]));

            Body.Emit(OpCodes.Nop);
            var generics = CreateGenerics();

            var argArrayBuilder = CreateArray(typeof(MethodArg), ParameterTypes.Length);
            for (var i = 0; i < ParameterTypes.Length; i++)
            {
                AddParameterToInputArray(i, argArrayBuilder);
            }

            // argsForCall = argArrayBuilder
            Body.Emit(OpCodes.Ldloc, argArrayBuilder);
            Body.Emit(OpCodes.Stloc, argsForCall);

            var ifResult = CallMockedMethod(generics, argsForCall, methodOut);

            // if (!ifResult) => onFailure
            Body.Emit(OpCodes.Ldloc, ifResult);
            Body.Emit(OpCodes.Brfalse_S, onFailure);

            OnSuccess(argsForCall, onReturn);

            Body.MarkLabel(onFailure);
            OnFailure(methodOut, onReturn);

            Body.MarkLabel(onReturn);
            OnReturn(methodOut);

            if (ParentMethod.DeclaringType.IsInterface)
                TypeBuilder.DefineMethodOverride(Method, ParentMethod);
        }

        public LocalBuilder CreateGenerics()
        {
            var generics = CreateArray(typeof(Type), Generics.Length);
            for (var i = 0; i < Generics.Length; i++)
            {
                // generics[i] = typeof(TParamater);
                Body.Emit(OpCodes.Ldloc, generics);
                Body.Emit(OpCodes.Ldc_I4, i);
                Body.TypeOf(Generics[i]);
                Body.Emit(OpCodes.Stelem_Ref);
            }

            return generics;
        }
    }
}