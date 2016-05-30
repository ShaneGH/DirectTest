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
    /// Base class to set a property or field
    /// </summary>
    public abstract class SetterBuilder : ILBuilder
    {        
        protected readonly ILGenerator MethodBody;
        readonly string FieldOrPropertyName;
        readonly Type FieldOrPropertyType;

        public SetterBuilder(ILGenerator methodBody, string fieldOrPropertyName, Type fieldOrPropertyType)
        {
            FieldOrPropertyName = fieldOrPropertyName;
            FieldOrPropertyType = fieldOrPropertyType;
            MethodBody = methodBody;
        }

        protected override void _Build()
        {
            var types = new[] { FieldOrPropertyType };
            var endFieldSetting = MethodBody.DefineLabel();

            // if (!ObjectBase.HasFieldOrProperty<T>("Name")) GO TO: next property
            MethodBody.Emit(OpCodes.Ldarg_1);
            MethodBody.Emit(OpCodes.Ldstr, FieldOrPropertyName);
            MethodBody.Emit(OpCodes.Call, ObjectBase.Reflection.HasMockedFieldOrProperty.MakeGenericMethod(types));
            MethodBody.Emit(OpCodes.Ldc_I4_0);
            MethodBody.Emit(OpCodes.Ceq);
            MethodBody.Emit(OpCodes.Brtrue, endFieldSetting);

            // this.Prop = ObjectBase.GetProperty<TProp>("Prop", true)
            MethodBody.Emit(OpCodes.Ldarg_0);
            MethodBody.Emit(OpCodes.Ldarg_1);
            MethodBody.Emit(OpCodes.Ldstr, FieldOrPropertyName);
            MethodBody.Emit(OpCodes.Ldc_I4_1);
            MethodBody.Emit(OpCodes.Call, ObjectBase.Reflection.GetProperty.MakeGenericMethod(types));

            DoSet();

            MethodBody.MarkLabel(endFieldSetting);
        }

        protected abstract void DoSet();
    }
}
