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
    public class DynamicBag : DynamicObject
    {
        public static readonly IEqualityComparer<IEnumerable<object>> ArrayComparer = Compare.Instance;

        readonly ConcurrentDictionary<IEnumerable<object>, object> _IndexedValues;
        readonly ConcurrentDictionary<string, object> _Values;
        public readonly ReadOnlyDictionary<string, object> Values;
        public readonly ReadOnlyDictionary<IEnumerable<object>, object> IndexedValues; 

        public DynamicBag()
            : this(new Dictionary<string, object>(), new Dictionary<IEnumerable<object>, object>())
        { }

        DynamicBag(IDictionary<string, object> initialialValues, IDictionary<IEnumerable<object>, object> indexedValues)
        {
            _Values = new ConcurrentDictionary<string, object>(initialialValues);
            _IndexedValues = new ConcurrentDictionary<IEnumerable<object>, object>(indexedValues, ArrayComparer);
            Values = new ReadOnlyDictionary<string, object>(_Values);
            IndexedValues = new ReadOnlyDictionary<IEnumerable<object>, object>(_IndexedValues);
        }

        protected void SetMember(string name, object value)
        {
            _Values.AddOrUpdate(name, value, (a, b) => value);
        }

        protected void SetIndex(IEnumerable<object> key, object value)
        {
            _IndexedValues.AddOrUpdate(key, value, (a, b) => value);
        }

        protected bool TryGetMember(string name, out object result)
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
            return _IndexedValues.TryGetValue(indexes, out result);
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
                return string.Join("-", (obj ?? Enumerable.Empty<object>()).Select(a => a.GetHashCode().ToString())).GetHashCode();
            }
        }
    }
}
