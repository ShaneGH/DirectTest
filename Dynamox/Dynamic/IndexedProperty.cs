using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;

namespace Dynamox.Dynamic
{
    /// <summary>
    /// Stores the keys and value of an indexed property 
    /// </summary>
    public class IndexedProperty
    {
        public readonly IEnumerable<object> Keys;
        public readonly object Value;

        public IndexedProperty(IEnumerable<object> keys, object value)
        {
            if (keys == null || !keys.Any())
                throw new InvalidOperationException("Invalid keys");

            Keys = keys.ToArray();
            Value = value;
        }

        /// <summary>
        /// Compare the keys of this object to the input keys, taking into account AnyValue instances if necessary
        /// </summary>
        /// <param name="treatAnyValueAsMatch">If set to true, will match an AnyValue class to any key it is compared to</param>
        public bool CompareKeys(IEnumerable<object> keys, bool treatAnyValueAsMatch = true)
        {
            if (keys == null)
                return false;

            var x = Keys.ToArray();
            var y = keys.ToArray();

            if (x.Length != y.Length)
                return false;

            for (var i = 0; i < x.Length; i++)
            {
                if (x[i] == null && y[i] == null)
                {
                    // do nothing
                }
                else if (treatAnyValueAsMatch && x[i] is AnyValue)
                {
                    if (!(x[i] as AnyValue).IsAnyValueType(y[i]))
                        return false;
                }
                else if (x[i] == null || !x[i].Equals(y[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
