using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks.Info
{
    /// <summary>
    /// A mocked property with callback methods on get and set
    /// </summary>
    public interface IPropertyMockBuilder<out TProperty>
    {
        /// <summary>
        /// Provide a callback to invoke when the property is accessed
        /// </summary>
        IPropertyMockBuilder<TProperty> OnGet(Action<TProperty> get);

        /// <summary>
        /// Provide a callback to invoke when the property is accessed
        /// </summary>
        IPropertyMockBuilder<TProperty> OnGet(Action get);

        /// <summary>
        /// Provide a callback to invoke when the property is accessed. The first argument of the callback is the old value of the property. The second argument is the new value.
        /// </summary>
        IPropertyMockBuilder<TProperty> OnSet(Action<TProperty, TProperty> set);

        /// <summary>
        /// Provide a callback to invoke when the property is accessed
        /// </summary>
        IPropertyMockBuilder<TProperty> OnSet(Action set);

        /// <summary>
        /// Ensure that this property will be accessed over the course of the test
        /// </summary>
        IPropertyMockBuilder<TProperty> DxEnsure();
    }
}
