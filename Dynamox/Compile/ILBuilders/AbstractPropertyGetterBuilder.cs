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
    /// Build a getter for a property which overrides an abstract parent property
    /// </summary>
    public class AbstractPropertyGetterBuilder : PropertyBuilder
    {
        readonly string PropertyName;

        public AbstractPropertyGetterBuilder(TypeBuilder toType, FieldInfo objBase, PropertyInfo parentProperty)
            : base(toType, objBase, parentProperty.GetMethod)
        {
            if (parentProperty.GetMethod == null)
                throw new CompilerException(toType.BaseType, "Cannot overrided getter for property " + parentProperty.Name);

            PropertyName = parentProperty.Name;
        }

        protected override LocalBuilder CallMockedMethod(LocalBuilder generics, LocalBuilder args, LocalBuilder methodOut)
        {
            var ifResult = Body.DeclareLocal(typeof(bool));

            // this.ObjectBase.GetProperty<TProperty>("PropertyName", true)
            Body.Emit(OpCodes.Ldarg_0);
            Body.Emit(OpCodes.Ldfld, ObjBase);
            Body.Emit(OpCodes.Ldstr, PropertyName);
            Body.Emit(OpCodes.Ldc_I4_1);
            Body.Emit(OpCodes.Call, ObjectBase.Reflection.GetProperty.MakeGenericMethod(ParentMethod.ReturnType));
            Body.Emit(OpCodes.Stloc, methodOut);

            // ifResult = true
            Body.Emit(OpCodes.Ldc_I4_1);
            Body.Emit(OpCodes.Stloc, ifResult);

            return ifResult;
        }
    }
}