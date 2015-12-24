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
                Body.Emit(OpCodes.Newobj, mockType.GetConstructor(new [] { ParameterTypes[index] }));
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


        /*.method public hidebysig virtual instance int32 
        DoSomething(int32 var1,
                    string var2,
                    [out] bool& var3,
                    string& var4,
                    [out] valuetype [mscorlib]System.Decimal& var5) cil managed
{
  // Code size       212 (0xd4)
  .maxstack  6
  .locals init ([0] int32 result,
           [1] class Dynamox.Mocks.MethodArg[] args,
           [2] int32 CS$1$0000,
           [3] class Dynamox.Mocks.MethodArg[] CS$0$0001,
           [4] bool CS$4$0002)
  IL_0000:  nop
  IL_0001:  ldc.i4.5
  IL_0002:  newarr     Dynamox.Mocks.MethodArg
  IL_0007:  stloc.3
  IL_0008:  ldloc.3
  IL_0009:  ldc.i4.0
  IL_000a:  ldtoken    [mscorlib]System.Int32
  IL_000f:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
  IL_0014:  ldarg.1
  IL_0015:  box        [mscorlib]System.Int32
  IL_001a:  newobj     instance void Dynamox.Mocks.MethodArg::.ctor(class [mscorlib]System.Type,
                                                                    object)
  IL_001f:  stelem.ref
  IL_0020:  ldloc.3
  IL_0021:  ldc.i4.1
  IL_0022:  ldtoken    [mscorlib]System.String
  IL_0027:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
  IL_002c:  ldarg.2
  IL_002d:  newobj     instance void Dynamox.Mocks.MethodArg::.ctor(class [mscorlib]System.Type,
                                                                    object)
  IL_0032:  stelem.ref
  IL_0033:  ldloc.3
  IL_0034:  ldc.i4.2
  IL_0035:  ldtoken    [mscorlib]System.Boolean
  IL_003a:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
  IL_003f:  ldnull
  IL_0040:  newobj     instance void Dynamox.Mocks.MethodArg::.ctor(class [mscorlib]System.Type,
                                                                    object)
  IL_0045:  stelem.ref
  IL_0046:  ldloc.3
  IL_0047:  ldc.i4.3
  IL_0048:  ldtoken    [mscorlib]System.String
  IL_004d:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
  IL_0052:  ldarg.s    var4
  IL_0054:  ldind.ref
  IL_0055:  newobj     instance void Dynamox.Mocks.MethodArg::.ctor(class [mscorlib]System.Type,
                                                                    object)
  IL_005a:  stelem.ref
  IL_005b:  ldloc.3
  IL_005c:  ldc.i4.4
  IL_005d:  ldtoken    [mscorlib]System.Int32
  IL_0062:  call       class [mscorlib]System.Type [mscorlib]System.Type::GetTypeFromHandle(valuetype [mscorlib]System.RuntimeTypeHandle)
  IL_0067:  ldnull
  IL_0068:  newobj     instance void Dynamox.Mocks.MethodArg::.ctor(class [mscorlib]System.Type,
                                                                    object)
  IL_006d:  stelem.ref
  IL_006e:  ldloc.3
  IL_006f:  stloc.1
  IL_0070:  ldarg.0
  IL_0071:  ldstr      "DoSomething"
  IL_0076:  ldloc.1
  IL_0077:  ldloca.s   result
  IL_0079:  call       instance bool Dynamox.Mocks.ObjectBase::TryInvoke<int32>(string,
                                                                                class [mscorlib]System.Collections.Generic.IEnumerable`1<class Dynamox.Mocks.MethodArg>,
                                                                                !!0&)
  IL_007e:  ldc.i4.0
  IL_007f:  ceq
  IL_0081:  stloc.s    CS$4$0002
  IL_0083:  ldloc.s    CS$4$0002
  IL_0085:  brtrue.s   IL_00be
  IL_0087:  nop
  IL_0088:  ldarg.3
  IL_0089:  ldloc.1
  IL_008a:  ldc.i4.2
  IL_008b:  ldelem.ref
  IL_008c:  ldfld      object Dynamox.Mocks.MethodArg::Arg
  IL_0091:  unbox.any  [mscorlib]System.Boolean
  IL_0096:  stind.i1
  IL_0097:  ldarg.s    var4
  IL_0099:  ldloc.1
  IL_009a:  ldc.i4.3
  IL_009b:  ldelem.ref
  IL_009c:  ldfld      object Dynamox.Mocks.MethodArg::Arg
  IL_00a1:  castclass  [mscorlib]System.String
  IL_00a6:  stind.ref
  IL_00a7:  ldarg.s    var5
  IL_00a9:  ldloc.1
  IL_00aa:  ldc.i4.3
  IL_00ab:  ldelem.ref
  IL_00ac:  ldfld      object Dynamox.Mocks.MethodArg::Arg
  IL_00b1:  unbox.any  [mscorlib]System.Decimal
  IL_00b6:  stobj      [mscorlib]System.Decimal
  IL_00bb:  nop
  IL_00bc:  br.s       IL_00ce
  IL_00be:  nop
  IL_00bf:  ldarg.0
  IL_00c0:  ldarg.1
  IL_00c1:  ldarg.2
  IL_00c2:  ldarg.3
  IL_00c3:  ldarg.s    var4
  IL_00c5:  ldarg.s    var5
  IL_00c7:  call       instance int32 Dynamox.Mocks.XXX::DoSomething(int32,
                                                                     string,
                                                                     bool&,
                                                                     string&,
                                                                     valuetype [mscorlib]System.Decimal&)
  IL_00cc:  stloc.0
  IL_00cd:  nop
  IL_00ce:  ldloc.0
  IL_00cf:  stloc.2
  IL_00d0:  br.s       IL_00d2
  IL_00d2:  ldloc.2
  IL_00d3:  ret
} // end of method ObjectBase::DoSomething



*/
    }
}
