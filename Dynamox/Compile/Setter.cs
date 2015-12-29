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
    public abstract class Setter : ILBuilderBase
    {
        static readonly MethodInfo HasMockedFieldOrProperty = typeof(ObjectBase).GetMethod("HasMockedFieldOrProperty");
        static readonly MethodInfo GetProperty = typeof(ObjectBase).GetMethod("GetProperty");
        
        protected readonly ILGenerator MethodBody;
        readonly string FieldOrPropertyName;
        readonly Type FieldOrPropertyType;

        public Setter(ILGenerator methodBody, string fieldOrPropertyName, Type fieldOrPropertyType)
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
            MethodBody.Emit(OpCodes.Call, HasMockedFieldOrProperty.MakeGenericMethod(types));
            MethodBody.Emit(OpCodes.Ldc_I4_0);
            MethodBody.Emit(OpCodes.Ceq);
            MethodBody.Emit(OpCodes.Brtrue, endFieldSetting);

            // this.Prop = ObjectBase.GetProperty<TProp>("Prop")
            MethodBody.Emit(OpCodes.Ldarg_0);
            MethodBody.Emit(OpCodes.Ldarg_1);
            MethodBody.Emit(OpCodes.Ldstr, FieldOrPropertyName);
            MethodBody.Emit(OpCodes.Call, GetProperty.MakeGenericMethod(types));

            DoSet();

            MethodBody.MarkLabel(endFieldSetting);
        }

        protected abstract void DoSet();
    }
}
