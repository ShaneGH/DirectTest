using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests
{
    public class TestBag : DynamicObject
    {
        readonly ConcurrentDictionary<string, object> _Values;
        public readonly ReadOnlyDictionary<string, object> Values; 

        public TestBag()
            : this(new Dictionary<string, object>())
        { }

        TestBag(IDictionary<string, object> initialialValues)
        {
            _Values = new ConcurrentDictionary<string,object>(initialialValues);
            Values = new ReadOnlyDictionary<string, object>(_Values);
        }

        protected virtual IDictionary<string, object> _Copy()
        {
            return _Values;
        }

        public TestBag Copy()
        {
            return new TestBag(_Copy());
        }

        protected void SetMember(string name, object value)
        {
            _Values.AddOrUpdate(name, value, (a, b) => value);
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

        protected void Clear() 
        {
            _Values.Clear();
        }
    }
}
