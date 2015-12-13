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
    /// Dumb cass which is not not thread safe
    /// </summary>
    public class VirtualMethodBuilderWithReturn : MethodBuilder
    {
        public VirtualMethodBuilderWithReturn(TypeBuilder toType, FieldInfo objBase, MethodInfo parentMethod)
            : base(toType, objBase, parentMethod)
        {
        }

        protected override LocalBuilder CallMockedMethod(LocalBuilder generics, LocalBuilder args, LocalBuilder methodOut)
        {
            var ifResult = Body.DeclareLocal(typeof(bool));

            // this.ObjectBase.TryInvoke<TVal>("MethodName", generics, args, out methodOut);
            Body.Emit(OpCodes.Ldarg_0);
            Body.Emit(OpCodes.Ldfld, ObjBase);
            Body.Emit(OpCodes.Ldstr, ParentMethod.Name);
            Body.Emit(OpCodes.Ldloc, generics);
            Body.Emit(OpCodes.Ldloc, args);
            Body.Emit(OpCodes.Ldloca, methodOut);
            Body.Emit(OpCodes.Call, ObjectBase.Reflection.TryInvokeGenericReturnValue.MakeGenericMethod(ParentMethod.ReturnType));

            // ifResult = topOfStack == 1
            Body.Emit(OpCodes.Ldc_I4_1);
            Body.Emit(OpCodes.Ceq);
            Body.Emit(OpCodes.Stloc, ifResult);

            return ifResult;
        }
    }
}