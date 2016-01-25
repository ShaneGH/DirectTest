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
    /// Build a getter for an indexed property which overrides an abstract parent property
    /// </summary>
    public class AbstractIndexGetterBuilder : PropertyBuilder
    {
        readonly string PropertyName;

        public AbstractIndexGetterBuilder(TypeBuilder toType, FieldInfo objBase, PropertyInfo parentProperty)
            : base(toType, objBase, parentProperty.GetMethod)
        {
            if (parentProperty.GetMethod == null)
                throw new CompilerException(toType.BaseType, "Cannot overrided getter for index.");

            PropertyName = parentProperty.Name;
        }

        protected override LocalBuilder CallMockedMethod(LocalBuilder generics, LocalBuilder args, LocalBuilder methodOut)
        {
            var ifResult = Body.DeclareLocal(typeof(bool));

            // this.ObjectBase.GetIndex<TProperty>(indexValues)
            Body.Emit(OpCodes.Ldarg_0);
            Body.Emit(OpCodes.Ldfld, ObjBase);
            Body.Emit(OpCodes.Ldloc, args);
            Body.Emit(OpCodes.Call, ObjectBase.Reflection.GetIndex.MakeGenericMethod(ParentMethod.ReturnType));
            Body.Emit(OpCodes.Stloc, methodOut);

            // ifResult = true
            Body.Emit(OpCodes.Ldc_I4_1);
            Body.Emit(OpCodes.Stloc, ifResult);

            return ifResult;
        }
    }
}