using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Dynamic
{
    public class IndexedIndexedValue
    {
        public readonly object Value;
        public readonly int Index;

        public IndexedIndexedValue(object value, int index)
        {
            Value = value;
            Index = index;
        }
    }

    public class DynamicBag : DynamicObject
    {
        public static readonly IEqualityComparer<IEnumerable<object>> ArrayComparer = Compare.Instance;

        readonly ConcurrentDictionary<IEnumerable<object>, IndexedIndexedValue> _IndexedValues;
        readonly ConcurrentDictionary<string, object> _Values;
        public readonly ReadOnlyDictionary<string, object> Values;
        public readonly ReadOnlyDictionary<IEnumerable<object>, IndexedIndexedValue> IndexedValues; 

        public DynamicBag()
            : this(new Dictionary<string, object>(), new Dictionary<IEnumerable<object>, IndexedIndexedValue>())
        { }

        DynamicBag(IDictionary<string, object> initialialValues, IDictionary<IEnumerable<object>, IndexedIndexedValue> indexedValues)
        {
            _Values = new ConcurrentDictionary<string, object>(initialialValues);
            _IndexedValues = new ConcurrentDictionary<IEnumerable<object>, IndexedIndexedValue>(indexedValues, ArrayComparer);
            Values = new ReadOnlyDictionary<string, object>(_Values);
            IndexedValues = new ReadOnlyDictionary<IEnumerable<object>, IndexedIndexedValue>(_IndexedValues);
        }

        protected internal void SetMember(string name, object value)
        {
            _Values.AddOrUpdate(name, value, (a, b) => value);
        }

        protected internal void SetIndex(IEnumerable<object> key, object value)
        {
            var val = new IndexedIndexedValue(value, _IndexedValues.Any() ?
                _IndexedValues.Max(v => v.Value.Index) + 1 :
                0);

            _IndexedValues.AddOrUpdate(key, val, (a, b) => val);
        }

        protected internal bool TryGetMember(string name, out object result)
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
            IndexedIndexedValue tmp;
            var output = _IndexedValues.TryGetValue(indexes, out tmp);

            result = !output || tmp == null ? null : tmp.Value;
            return output;
        }

        protected void Clear() 
        {
            _Values.Clear();
        }

        /// <summary>
        /// TODO: test, TODO: centralise, TODO: I'm pretty sure this functionality is duplicates somewhere else
        /// </summary>
        private class Compare : IEqualityComparer<IEnumerable<object>>
        {
            public static readonly Compare Instance = new Compare();

            private Compare() { }

            public bool Equals(IEnumerable<object> x, IEnumerable<object> y)
            {
                if (x == null && y == null)
                    return true;

                if (x == null || y == null)
                    return false;

                var _x = x.ToArray();
                var _y = y.ToArray();


                if (_x.Length != _y.Length)
                    return false;

                for (var i = 0; i < _x.Length; i++)
                    if (_x[i] == null && _y[i] == null) ;
                    else if (_x[i] == null || !_x[i].Equals(_y[i]))
                        return false;

                return true;
            }

            public int GetHashCode(IEnumerable<object> obj)
            {
                return string.Join("-", (obj ?? Enumerable.Empty<object>()).Select(a => a == null ? "0" : a.GetHashCode().ToString())).GetHashCode();
            }
        }
    }
}
