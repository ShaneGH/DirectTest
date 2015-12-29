using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks.Info
{
    /// <summary>
    /// A mocked property which can be accessed or set
    /// </summary>
    public interface IPropertyMockAccessor
    {
        /// <summary>
        /// Get the property
        /// </summary>
        TProperty Get<TProperty>();

        /// <summary>
        /// Set the property
        /// </summary>
        void Set(object value);
    }
}
