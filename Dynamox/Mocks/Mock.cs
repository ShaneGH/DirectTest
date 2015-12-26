using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            if (mockType.IsSealed && (!settings.CreateSealedClassesWithEmptyConstructors || !ObjectBase.HasEmptyConstructor(mockType)))
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
                    if (MockType.IsSealed && (!Settings.CreateSealedClassesWithEmptyConstructors || !ObjectBase.HasEmptyConstructor(MockType)))
                        throw new InvalidOperationException();  //TODE

                    if (!MockType.IsSealed)
                    {
                        Constructors.Add(MockType, BuildConstructorForMock(MockType));
                    }
                    else
                    {
                        Constructors.Add(MockType, BuildConstructorForNonMock(MockType));
                    }
                }
            }
        }

        static Func<ObjectBase, object> BuildConstructorForMock(Type mockType)
        {
            var compiled = Compiler.Compile(mockType);
            var param = Expression.Parameter(typeof(ObjectBase));
            return Expression.Lambda<Func<ObjectBase, object>>(
                    Expression.Convert(Expression.New(compiled.GetConstructor(new[] { typeof(ObjectBase) }), param), mockType), param).Compile();
        }

        static readonly MethodInfo HasMockedFieldOrProperty = typeof(ObjectBase).GetMethod("HasMockedFieldOrProperty");
        static readonly MethodInfo GetProperty = typeof(ObjectBase).GetMethod("GetProperty");
        static Func<ObjectBase, object> BuildConstructorForNonMock(Type mockType)
        {
            var type = TypeOverrideDescriptor.Create(mockType);
            var mock = Expression.Variable(mockType, "asdsadasd");
            var values = Expression.Parameter(typeof(ObjectBase));

            var constructed = (Expression)Expression.Assign(mock, Expression.New(mockType));

            var setters = type.SettableProperties.Where(p => p.SetMethod.IsPublic).Select(p => new { p.Name, type = p.PropertyType })
                .Concat(type.SettableFields.Where(f => f.IsPublic).Select(f => new { f.Name, type = f.FieldType }))
                .Select(p => Expression.IfThen(
                    // if (values.HasMockedFieldOrProperty<T>(name))
                    Expression.Call(values, HasMockedFieldOrProperty.MakeGenericMethod(new []{p.type}), Expression.Constant(p.Name)),
                    // mock.name = values.GetProperty<T>(name);
                    Expression.Assign(Expression.PropertyOrField(mock, p.Name), Expression.Call(values, GetProperty.MakeGenericMethod(new []{p.type}), Expression.Constant(p.Name)))));

            return Expression.Lambda<Func<ObjectBase, object>>(
                Expression.Block(new[] { mock }, new [] { constructed }.Concat(setters).Concat(new[] { mock })), values).Compile();
        }

        object BuildObject()
        {
            Compile();

            var obj = new ObjectBase(Settings, Members, Indexes);
            if (Settings.TestForInvalidMocks)
            {
                var errors = ObjectBaseValidator.Create(MockType).ValidateAgainstType(obj);
                if (errors.Any())
                    throw new InvalidOperationException(errors.Count().ToString());  //TODE
            }

            return Constructors[MockType](obj);
        }
    }
}