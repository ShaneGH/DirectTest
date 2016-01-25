using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks.Info
{
    /// <summary>
    /// A mocked property with get and set functionality
    /// </summary>
    /// <typeparam name="TProperty">The property type</typeparam>
    internal class PropertyMockBuilder<TProperty> : IPropertyMockBuilder<TProperty>, IPropertyMockAccessor
    {
        Func<TProperty> GetProperty { get; set; }

        readonly List<Action<TProperty, TProperty>> OnSetActions = new List<Action<TProperty, TProperty>>();
        readonly List<Action<TProperty>> OnGetActions = new List<Action<TProperty>>();
        readonly bool CanSet;

        public PropertyMockBuilder()
            : this(default(TProperty))
        {
        }

        public PropertyMockBuilder(TProperty propertyValue)
            : this(() => propertyValue, true)
        {
        }

        public PropertyMockBuilder(Func<TProperty> propertyValue, bool canSet = false)
        {
            CanSet = canSet;
            GetProperty = propertyValue;
        }

        public IPropertyMockBuilder<TProperty> OnGet(Action<TProperty> get)
        {
            OnGetActions.Add(get);
            return this;
        }

        public IPropertyMockBuilder<TProperty> OnGet(Action get)
        {
            return OnGet(a => get());
        }

        public IPropertyMockBuilder<TProperty> OnSet(Action<TProperty, TProperty> set)
        {
            OnSetActions.Add(set);
            return this;
        }

        public IPropertyMockBuilder<TProperty> OnSet(Action set)
        {
            return OnSet((a, b) => set());
        }

        public T Get<T>()
        {
            if (!typeof(T).IsAssignableFrom(typeof(TProperty)))
                throw new InvalidMockException("Property of type " + typeof(TProperty) + " cannot be converted to " + typeof(T));

            var property = GetProperty();
            foreach (var p in OnGetActions)
                p(property);

            return (T)(object)property;
        }

        public void Set(object value)
        {
            if (!CanSet)
                return;

            if (value == null)
            {
                if (typeof(TProperty).IsValueType)
                    throw new InvalidMockException("Property of type " + typeof(TProperty) + " cannot be assigned from null");
            }
            else if (!typeof(TProperty).IsAssignableFrom(value.GetType()))
            {
                throw new InvalidMockException("Property of type " + typeof(TProperty) + " cannot be converted to " + value.GetType());
            }

            TProperty val = (TProperty)value;
            var property = GetProperty();
            foreach (var p in OnSetActions)
                p(property, val);

            GetProperty = () => val;
        }
    }
}
