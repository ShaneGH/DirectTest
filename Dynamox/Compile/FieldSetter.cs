using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Compile
{
    public class FieldSetter : Setter
    {
        readonly FieldInfo Field;

        public FieldSetter(ILGenerator methodBody, FieldInfo field)
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
