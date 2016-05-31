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
    interface IEnsuredProperty
    {
        /// <summary>
        /// The actual property value
        /// </summary>
        object Value { get; }

        /// <summary>
        /// If true, the value of this object is ensured, otherwise it is not
        /// </summary>
        bool IsEnsured { get; }
    }

    /// <summary>
    /// Flag for MockBuilder class to indicate that the property set should be "Ensured"
    /// </summary>
    internal class EnsuredProperty : IEnsuredProperty
    {
        /// <summary>
        /// The actual property value
        /// </summary>
        public readonly object Value;

        public EnsuredProperty(object value) 
        {
            Value = value;
        }

        object IEnsuredProperty.Value
        {
            get { return Value; }
        }

        bool IEnsuredProperty.IsEnsured
        {
            get { return true; }
        }
    }
}
