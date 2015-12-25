using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;

namespace Dynamox.Compile
{
    /// <summary>
    /// Build a method for a dynamic type based on a method in the parent class
    /// Dumb class which is not not thread safe
    /// </summary>
    public abstract class MethodBuilder
    {
        protected static readonly MethodInfo GetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle");
        protected static readonly FieldInfo MethodArg_Arg = typeof(MethodArg).GetField("Arg");

        protected readonly TypeBuilder ToType;
        protected readonly FieldInfo ObjBase;
        protected readonly MethodInfo ParentMethod;

        protected readonly GenericTypeParameterBuilder[] Generics;
        protected readonly ParameterInfo[] Parameters;
        protected readonly Type[] ParameterTypes;
        public readonly System.Reflection.Emit.MethodBuilder Method;
        protected readonly ILGenerator Body;

        public MethodBuilder(TypeBuilder toType, FieldInfo objBase, MethodInfo parentMethod)
        {
            ToType = toType;
            ObjBase = objBase;
            ParentMethod = parentMethod;

            Parameters = ParentMethod.GetParameters();
            ParameterTypes = Parameters.Select(p => p.ParameterType).ToArray();
            Method = ToType.DefineMethod(ParentMethod.Name, GetAttrs(ParentMethod), ParentMethod.ReturnType, ParameterTypes);
            Body = Method.GetILGenerator();

            if (!ParentMethod.ContainsGenericParameters)
            {
                Generics = new GenericTypeParameterBuilder[0];
            }
            else
            {
                var generics = ParentMethod.GetGenericArguments();
                Generics = Method.DefineGenericParameters(generics.Select((g, i) => "T" + 1).ToArray());
            }
        }

        protected virtual MethodAttributes GetAttrs(MethodInfo forMethod)
        {
            var _base = MethodAttributes.HideBySig | MethodAttributes.Virtual;
            if (forMethod.IsPublic)
                _base = _base | MethodAttributes.Public;
            else if (forMethod.IsFamilyOrAssembly)
                _base = _base | MethodAttributes.FamORAssem;
            else if (forMethod.IsFamily)
                _base = _base | MethodAttributes.Family;
            else if (forMethod.IsAssembly)
                _base = _base | MethodAttributes.Assembly;
            else if (forMethod.IsFamilyAndAssembly)
                _base = _base | MethodAttributes.FamANDAssem;
            else if (forMethod.IsPrivate)
                _base = _base | MethodAttributes.Private;

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

        void AddParameterToInputArray(int index, LocalBuilder array)
        {
            var mockType = typeof(MethodArg<>).MakeGenericType(ParameterTypes[index]);

            Body.Emit(OpCodes.Ldloc, array);
            Body.Emit(OpCodes.Ldc_I4, index);

            if (Parameters[index].IsOut)
            {
                Body.Emit(OpCodes.Newobj, mockType.GetConstructor(Type.EmptyTypes));
            }
            else
            {
                Body.Emit(OpCodes.Ldarg, index + 1);
                Body.Emit(OpCodes.Newobj, mockType.GetConstructor(new[] { ParameterTypes[index] }));
            }

            // = new MethodArg<T>(value);
            Body.Emit(OpCodes.Stelem_Ref);
        }

        protected abstract LocalBuilder CallMockedMethod(LocalBuilder generics, LocalBuilder args, LocalBuilder methodOut);

        void AssignOutParameter(int argumentIndex, LocalBuilder args)
        {
            // arg_i
            Body.Emit(OpCodes.Ldarg, argumentIndex + 1);

            // args[i].Arg
            Body.Emit(OpCodes.Ldloc, args);
            Body.Emit(OpCodes.Ldc_I4, argumentIndex);
            Body.Emit(OpCodes.Ldelem_Ref);
            Body.Emit(OpCodes.Ldfld, MethodArg_Arg);

            // arg_i = args[i].Arg;
            if (ParameterTypes[argumentIndex].IsValueType)
            {
                Body.Emit(OpCodes.Unbox_Any, ParameterTypes[argumentIndex]);
                Body.Emit(OpCodes.Stobj, ParameterTypes[argumentIndex]);
            }
            else
            {
                if (ParameterTypes[argumentIndex] != typeof(object))
                    Body.Emit(OpCodes.Castclass, ParameterTypes[argumentIndex]);
                Body.Emit(OpCodes.Stind_Ref);   //TODO: if it is a ref parameter, does setting the value in the method change the output (value and ref types)
            }
        }

        public void OnSuccess(LocalBuilder args, Label onReturn)
        {
            Body.Emit(OpCodes.Nop);
            for (var i = 0; i < Parameters.Length; i++)
            {
                if (!Parameters[i].IsOut && !Parameters[i].ParameterType.IsByRef) //TODO: test
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

        bool Built = false;
        readonly object BuildLock = new object();
        public void Build()
        {
            lock (BuildLock)
            {
                if (Built)
                    throw new InvalidOperationException();  //TODO
                Built = true;
            }

            if (!ParentMethod.IsAbstract && !ParentMethod.IsVirtual)
                throw new InvalidOperationException();  //TODO

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
        }

        public LocalBuilder CreateGenerics()
        {
            var generics = CreateArray(typeof(Type), Generics.Length);
            for (var i = 0; i < Generics.Length; i++)
            {
                // generics[i] = typeof(TParamater);
                Body.Emit(OpCodes.Ldloc, generics);
                Body.Emit(OpCodes.Ldc_I4, i);
                Body.Emit(OpCodes.Ldtoken, Generics[i]);
                Body.Emit(OpCodes.Call, GetTypeFromHandle);
                Body.Emit(OpCodes.Stelem_Ref);
            }

            return generics;
        }
    }
}