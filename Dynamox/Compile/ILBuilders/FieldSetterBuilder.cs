using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Compile.ILBuilders
{
    /// <summary>
    /// Set a field
    /// </summary>
    public class FieldSetterBuilder : SetterBuilder
    {
        readonly FieldInfo Field;

        public FieldSetterBuilder(ILGenerator methodBody, FieldInfo field)
            : base(methodBody, field.Name, field.FieldType)
        {
            Field = field;
        }

        protected override void DoSet()
        {
            MethodBody.Emit(OpCodes.Stfld, Field);
        }
    }
}
