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
    /// Set a property
    /// </summary>
    public class PropertySetterBuilder : SetterBuilder
    {
        readonly PropertyInfo Property;

        public PropertySetterBuilder(ILGenerator methodBody, PropertyInfo property)
            : base(methodBody, property.Name, property.PropertyType)
        {
            Property = property;
        }

        protected override void DoSet()
        {
            MethodBody.Emit(OpCodes.Call, Property.SetMethod);
        }
    }
}
