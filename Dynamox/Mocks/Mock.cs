using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Dynamox.Compile;

namespace Dynamox.Mocks
{
    internal class Mock
    {
        public readonly DxSettings Settings;
        public readonly Type MockType;
        public readonly ReadOnlyDictionary<string, object> Members;
        public readonly ReadOnlyDictionary<IEnumerable<object>, object> Indexes;

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

        public Mock(object value, DxSettings settings)
            : this(settings)
        {
            _Object = value;
        }

        public Mock(Type mockType, MockBuilder builder, DxSettings settings)
            : this(settings)
        {
            if (mockType.IsSealed)
                throw new InvalidOperationException("Cannot mock sealed");  //TODE

            MockType = mockType;
            Members = builder.Values;
            Indexes = builder.IndexedValues;
        }

        /// <summary>
        /// Must be used in conjunction with another constructor
        /// </summary>
        /// <param name="settings"></param>
        private Mock(DxSettings settings)
        {
            Settings = settings;
        }

        private static readonly Dictionary<Type, Func<ObjectBase, object>> Constructors = new Dictionary<Type, Func<ObjectBase, object>>();
        void Compile()
        {
            lock (Constructors)
            {
                if (!Constructors.ContainsKey(MockType))
                {
                    var compiled = Compiler.Compile(MockType);
                    var param = Expression.Parameter(typeof(ObjectBase));
                    Constructors.Add(MockType, 
                        Expression.Lambda<Func<ObjectBase, object>>(
                            Expression.Convert(Expression.New(compiled.GetConstructor(new[] { typeof(ObjectBase) }), param), MockType), param).Compile());
                }
            }
        }

        object BuildObject()
        {
            Compile();

            return Constructors[MockType](new ObjectBase(Settings, Members, Indexes));
        }
    }
}