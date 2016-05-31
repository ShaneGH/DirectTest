using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dynamox.Compile;
using Dynamox.Mocks.Info;

namespace Dynamox.Mocks
{
    /// <summary>
    /// A mock, which takes in a mock builder and the type to mock and returns a proxy object of that type
    /// </summary>
    internal class Mock
    {
        public readonly DxSettings Settings;
        public readonly Type MockType;
        public readonly IEnumerable<object> ConstructorArgs;
        public readonly MockBuilder MockInfo;

        private static readonly object UnAssigned = new object();
        private Object _Object = UnAssigned;
        public Object Object
        {
            get
            {
                return _Object != UnAssigned ? _Object : (_Object = BuildObject(ConstructorArgs));
            }
            set
            {
                _Object = value;
            }
        }

        public Mock(Type mockType, MockBuilder builder, DxSettings settings, IEnumerable<object> constructorArgs = null)
        {
            if (mockType.IsSealed && !settings.CreateSealedClasses)
                throw new InvalidOperationException("Cannot mock a sealed class" + mockType.Name);

            ConstructorArgs = constructorArgs ?? Enumerable.Empty<object>();
            Settings = settings;
            MockType = mockType;
            MockInfo = builder;
        }

        private static readonly Dictionary<Type, Constructors> Constructors = new Dictionary<Type, Constructors>();
        void Compile()
        {
            lock (Constructors)
            {
                if (!Constructors.ContainsKey(MockType))
                {
                    var compiled = MockType.IsSealed ? MockType : Compiler.Compile(MockType);
                    Constructors.Add(MockType, new Constructors(compiled));
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

        static readonly MethodInfo HasMockedFieldOrProperty = ObjectBase.Reflection.HasMockedFieldOrProperty;
        static readonly MethodInfo GetProperty = ObjectBase.Reflection.GetProperty;
        static Func<ObjectBase, object> BuildConstructorForNonMock(Type mockType)
        {
            var type = TypeOverrideDescriptor.Create(mockType);
            var mock = Expression.Variable(mockType);
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

        object BuildObject(IEnumerable<object> constructorArgs)
        {
            Compile();

            var obj = new ObjectBase(Settings, MockInfo);
            if (Settings.TestForInvalidMocks)
            {
                var errors = ObjectBaseValidator.Create(MockType).ValidateAgainstType(obj);
                if (errors.Any())
                    throw new InvalidMockException(string.Join(Environment.NewLine, new[] { "Errors detected when attempting to mock " + MockType }.Concat(errors)));
            }

            return Constructors[MockType].Construct(obj, constructorArgs);
        }
    }
}