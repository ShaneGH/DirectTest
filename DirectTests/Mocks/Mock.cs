using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
            if (!mockType.IsInterface)
                throw new NotImplementedException();

            MockType = mockType;
            Builder = builder.Values;
        }

        private readonly Dictionary<Type, ConstructorInfo> Constructors = new Dictionary<Type,ConstructorInfo>();
        void Compile() 
        {
            lock (Constructors)
            {
                // add constructor
            }
        }

        object BuildObject()
        {
            Compile();

            return Constructors[MockType].Invoke(new [] { new ObjectBase(Builder) });
        }
    }
}