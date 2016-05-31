using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks.Info
{
    /// <summary>
    /// Flag for MockBuilder class to indicate that the property set should be "Ensured"
    /// </summary>
    internal class EnsuredProperty
    {
        /// <summary>
        /// The actual property value
        /// </summary>
        public readonly object Value;

        public EnsuredProperty(object value) 
        {
            Value = value;
        }
    }
}
