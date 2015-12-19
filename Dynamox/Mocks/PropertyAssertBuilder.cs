using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    public interface IPropertyAssertAccessor
    {
        TProperty Get<TProperty>();
        void Set(object value);
    }

    public interface IPropertyAssertBuilder<TProperty>
    {
        IPropertyAssertBuilder<TProperty> OnGet(Action<TProperty> get);
        IPropertyAssertBuilder<TProperty> OnGet(Action get);

        IPropertyAssertBuilder<TProperty> OnSet(Action<TProperty, TProperty> set);
        IPropertyAssertBuilder<TProperty> OnSet(Action set);
    }

    internal class PropertyAssertBuilder<TProperty> : IPropertyAssertBuilder<TProperty>, IPropertyAssertAccessor
    {
        Func<TProperty> GetProperty { get; set; }

        readonly List<Action<TProperty, TProperty>> OnSetActions = new List<Action<TProperty, TProperty>>();
        readonly List<Action<TProperty>> OnGetActions = new List<Action<TProperty>>();
        readonly bool CanSet;

        public PropertyAssertBuilder()
            : this(default(TProperty))
        {
        }

        public PropertyAssertBuilder(TProperty propertyValue)
            : this(() => propertyValue, true)
        {
        }

        public PropertyAssertBuilder(Func<TProperty> propertyValue, bool canSet = false)
        {
            CanSet = canSet;
            GetProperty = propertyValue;
        }

        public IPropertyAssertBuilder<TProperty> OnGet(Action<TProperty> get)
        {
            OnGetActions.Add(get);
            return this;
        }

        public IPropertyAssertBuilder<TProperty> OnGet(Action get)
        {
            return OnGet(a => get());
        }

        public IPropertyAssertBuilder<TProperty> OnSet(Action<TProperty, TProperty> set)
        {
            OnSetActions.Add(set);
            return this;
        }

        public IPropertyAssertBuilder<TProperty> OnSet(Action set)
        {
            return OnSet((a, b) => set());
        }

        public T Get<T>()
        {
            if (!typeof(T).IsAssignableFrom(typeof(TProperty)))
                throw new InvalidOperationException();  //TODE

            var property = GetProperty();
            foreach (var p in OnGetActions)
                p(property);

            return (T)(object)property;
        }

        public void Set(object value)
        {
            if (!CanSet)
                return;

            if (!typeof(TProperty).IsAssignableFrom(value.GetType()))
                throw new InvalidOperationException();  //TODE

            TProperty val = (TProperty)value;
            var property = GetProperty();
            foreach (var p in OnSetActions)
                p(property, val);

            GetProperty = () => val;
        }
    }
}
