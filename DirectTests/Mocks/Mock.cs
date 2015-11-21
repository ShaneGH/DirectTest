using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectTests.Mocks;

namespace DirectTests.Mocks
{
    internal class Mock
    {
        public readonly Type MockType;
        public readonly ReadOnlyDictionary<string, object> Builder;

        private static readonly object UnAssigned = new object();
        private Object _Object = UnAssigned;
        public Object Object
        {
            get
            {
                return _Object != UnAssigned ? _Object : (_Object = BuildObject());
            }
            set
            {
                _Object = value;
            }
        }

        public Mock(object value)
        {
            _Object = value;
        }

        public Mock(Type mockType, MockBuilder builder)
        {
            MockType = mockType;
            Builder = builder.Values;
        }

        object BuildObject()
        {
            return @"
new (public class MyMock  : MockType
{
    public readonly IEnumerable<MemberDescription> Properties;
    public readonly IEnumerable<MethodDescription> Methods;

    public int MyProp
    {
        get
        {
            (int)Properties.First(p => p.Name == MyProp)
        }
    }
 
});
";
        }
    }
}