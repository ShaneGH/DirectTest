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
    /// Build a setter for a property which overrides a virtual parent property
    /// </summary>
    public class VirtualPropertySetterBuilder : PropertyBuilder
    {
        readonly string PropertyName;

        public VirtualPropertySetterBuilder(TypeBuilder toType, FieldInfo objBase, PropertyInfo parentProperty)
            : base(toType, objBase, parentProperty.SetMethod)
        {
            if (parentProperty.SetMethod == null)
                throw new InvalidOperationException("Cannot overrided getter"); //TODO

            PropertyName = parentProperty.Name;
        }

        protected override LocalBuilder CallMockedMethod(LocalBuilder generics, LocalBuilder args, LocalBuilder methodOut)
        {
            var ifResult = Body.DeclareLocal(typeof(bool));

            // this.ObjectBase.TrySetProperty<TProperty>("PropertyName", value)
            Body.Emit(OpCodes.Ldarg_0);
            Body.Emit(OpCodes.Ldfld, ObjBase);
            Body.Emit(OpCodes.Ldstr, PropertyName);

            Body.LoadArrayElement(args, 0);
            Body.Emit(OpCodes.Ldfld, MethodArg_Arg);

            Body.Emit(OpCodes.Call, ObjectBase.Reflection.TrySetProperty);

            // ifResult = topOfStack == 1
            Body.Emit(OpCodes.Ldc_I4_1);
            Body.Emit(OpCodes.Ceq);
            Body.Emit(OpCodes.Stloc, ifResult);

            return ifResult;
        }
    }
}