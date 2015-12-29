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
    /// Build a setter for a property which overrides an abstract parent property
    /// </summary>
    public class AbstractPropertySetterBuilder : PropertyBuilder
    {
        readonly string PropertyName;

        public AbstractPropertySetterBuilder(TypeBuilder toType, FieldInfo objBase, PropertyInfo parentProperty)
            : base(toType, objBase, parentProperty.SetMethod)
        {
            if (parentProperty.SetMethod == null)
                throw new InvalidOperationException("Cannot overrided setter"); //TODO

            PropertyName = parentProperty.Name;
        }

        protected override LocalBuilder CallMockedMethod(LocalBuilder generics, LocalBuilder args, LocalBuilder methodOut)
        {
            var ifResult = Body.DeclareLocal(typeof(bool));

            // this.ObjectBase.SetProperty("PropertyName", args[0].Arg)
            Body.Emit(OpCodes.Ldarg_0);
            Body.Emit(OpCodes.Ldfld, ObjBase);
            Body.Emit(OpCodes.Ldstr, PropertyName);

            Body.LoadArrayElement(args, 0);
            Body.Emit(OpCodes.Ldfld, MethodArg_Arg);

            Body.Emit(OpCodes.Call, ObjectBase.Reflection.SetProperty);

            // ifResult = true
            Body.Emit(OpCodes.Ldc_I4_1);
            Body.Emit(OpCodes.Stloc, ifResult);

            return ifResult;
        }
    }
}