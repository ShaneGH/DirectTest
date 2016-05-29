using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    /// <summary>
    /// An object which will match any comparrison for mocking purposes
    /// </summary>
    public class AnyValue
    {
        public readonly Type OfType;

        public AnyValue(Type ofType)
        {
            OfType = ofType;
        }

        public bool IsAnyValueType(object input)
        {
            return OfType == typeof(AnyValue) ||
                (input == null && !OfType.IsValueType) ||
                (input != null && OfType.IsAssignableFrom(input.GetType()));
        }
    }

    /// <summary>
    /// An object which will match any comparrison for mocking purposes
    /// </summary>
    public sealed class AnyValue<T> : AnyValue
    {
        public AnyValue()
            : base(typeof(T))
        {
        }

        public static implicit operator T(AnyValue<T> b)
        {
            return default(T);
        }
    }
}
