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
    /// Build a getter for an indexed property which overrides a virtual parent property
    /// </summary>
    public class VirtualIndexGetterBuilder : PropertyBuilder
    {
        readonly string PropertyName;

        public VirtualIndexGetterBuilder(TypeBuilder toType, FieldInfo objBase, PropertyInfo parentProperty)
            : base(toType, objBase, parentProperty.GetMethod)
        {
            if (parentProperty.GetMethod == null)
                throw new InvalidOperationException("Cannot overrided getter"); //TODO

            PropertyName = parentProperty.Name;
        }

        protected override LocalBuilder CallMockedMethod(LocalBuilder generics, LocalBuilder args, LocalBuilder methodOut)
        {
            var ifResult = Body.DeclareLocal(typeof(bool));

            // this.ObjectBase.TryGetIndex<TVal>(args, out methodOut);
            Body.Emit(OpCodes.Ldarg_0);
            Body.Emit(OpCodes.Ldfld, ObjBase);
            Body.Emit(OpCodes.Ldloc, args);
            Body.Emit(OpCodes.Ldloca, methodOut);
            Body.Emit(OpCodes.Call, ObjectBase.Reflection.TryGetIndex.MakeGenericMethod(ParentMethod.ReturnType));

            // ifResult = topOfStack == 1
            Body.Emit(OpCodes.Ldc_I4_1);
            Body.Emit(OpCodes.Ceq);
            Body.Emit(OpCodes.Stloc, ifResult);

            return ifResult;
        }
    }
}