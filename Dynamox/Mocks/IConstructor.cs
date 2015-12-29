using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    /// <summary>
    /// Attempt to construct a mocked object with the given constructor args
    /// </summary>
    public interface IConstructor
    {
        /// <summary>
        /// Try to build an object and return null if it is not possible
        /// </summary>
        object TryConstruct(ObjectBase objectBase, IEnumerable<object> otherArgs);
    }
}
