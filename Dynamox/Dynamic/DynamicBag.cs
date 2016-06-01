using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;

namespace Dynamox.Dynamic
{
    public class DynamicBag : DynamicObject
    {
        readonly List<IndexedProperty> _IndexedValues;
        readonly ConcurrentDictionary<string, object> _Values;
        public readonly ReadOnlyDictionary<string, object> Values;

        public IEnumerable<IndexedProperty> IndexedValues
        {
            get
            {
                return _IndexedValues.Skip(0);
            }
        }

        public DynamicBag()
            : this(new Dictionary<string, object>(), Enumerable.Empty<KeyValuePair<IEnumerable<object>, object>>())
        { }

        DynamicBag(IDictionary<string, object> initialialValues, IEnumerable<KeyValuePair<IEnumerable<object>, object>> indexedValues)
        {
            _Values = new ConcurrentDictionary<string, object>(initialialValues);
            _IndexedValues = new List<IndexedProperty>(indexedValues.Select(i => new IndexedProperty(i.Key, i.Value)));
            Values = new ReadOnlyDictionary<string, object>(_Values);
        }

        protected virtual internal void SetMember(string name, object value)
        {
            _Values.AddOrUpdate(name, value, (a, b) => value);
        }

        protected virtual internal void SetIndex(IEnumerable<object> key, object value)
        {
            // the later objects take precedence
            _IndexedValues.Insert(0, new IndexedProperty(key, value));
        }

        protected virtual internal bool TryGetMember(string name, out object result)
        {
            return _Values.TryGetValue(name, out result);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetMember(binder.Name, out result);
        }

        public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object value)
        {
            SetMember(binder.Name, value);
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            SetIndex(indexes, value);
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            var value = _IndexedValues.FirstOrDefault(i => i.CompareKeys(indexes));

            result = value == null ? null : value.Value;
            return result != null;
        }

        protected void Clear() 
        {
            _Values.Clear();
        }
    }
}
