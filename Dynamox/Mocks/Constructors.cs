using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Compile;

namespace Dynamox.Mocks
{
    public class Constructors : IEnumerable<Constructor>
    {
        readonly IEnumerable<Constructor> _Constructors;

        public Constructors(Type forType) 
        {
            if (forType.IsInterface) forType = typeof(object);

            _Constructors = Array.AsReadOnly(forType.GetConstructors()
                // put the empty constructor first, it is most likely to be used
                .OrderBy(c => c.GetParameters().Length)
                .Select(c => forType.IsSealed ?
                    new NonMockedConstructor(c) :
                    new Constructor(c)).ToArray());
        }

        public object TryConstruct(ObjectBase objectBase, IEnumerable<object> otherArgs)
        {
            return _Constructors.Select(c => c.TryConstruct(objectBase, otherArgs))
                .FirstOrDefault(c => c != null);
        }

        public object Construct(ObjectBase objectBase)
        {
            return Construct(objectBase, Enumerable.Empty<object>());
        }

        public object Construct(ObjectBase objectBase, IEnumerable<object> otherArgs)
        {
            var result = TryConstruct(objectBase, otherArgs);
            if (result == null)
                throw new InvalidOperationException();  //TODE

            return result;
        }

        public IEnumerator<Constructor> GetEnumerator()
        {
            return _Constructors.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _Constructors.GetEnumerator();
        }
    }

    public class Constructor
    {
        readonly IReadOnlyCollection<Type> Parameters;
        readonly Func<object[], object> _Constructor;

        public Constructor(ConstructorInfo constructor)
        {
            Parameters = Array.AsReadOnly(constructor.GetParameters().Select(p => p.ParameterType).ToArray());

            var inputs = Expression.Parameter(typeof(object[]));
            var create = Expression.New(constructor,
                    Parameters.Select((p, i) => Expression.Convert(Expression.ArrayIndex(inputs, Expression.Constant(i)), p)));

            _Constructor = Expression.Lambda<Func<object[], object>>(constructor.DeclaringType.IsValueType ?
                Expression.Convert(create, typeof(object)) as Expression : create, inputs).Compile();
        }

        public virtual object TryConstruct(ObjectBase objectBase, IEnumerable<object> otherArgs)
        {
            var inputs = (objectBase == null ? new object[0] : new[] { objectBase }).Concat(otherArgs).ToArray();
            if (inputs.Length != Parameters.Count)
                return null;

            for (var i = 0; i < inputs.Length; i++)
            {
                if (inputs[i] == null)
                {
                    if (Parameters.ElementAt(i).IsValueType)
                        return null;
                }
                else if (!Parameters.ElementAt(i).IsAssignableFrom(inputs[i].GetType()))
                {
                    return null;
                }
            }

            return _Constructor(inputs);
        }
    }

    public class NonMockedConstructor : Constructor
    {
        static readonly MethodInfo HasMockedFieldOrProperty = typeof(ObjectBase).GetMethod("HasMockedFieldOrProperty");
        static readonly MethodInfo GetProperty = typeof(ObjectBase).GetMethod("GetProperty");

        readonly Action<object, ObjectBase> Setter;

        public NonMockedConstructor(ConstructorInfo constructor)
            : base(constructor)
        {
            var type = TypeOverrideDescriptor.Create(constructor.DeclaringType);
            var mock = Expression.Variable(constructor.DeclaringType);
            var input = Expression.Parameter(typeof(object));
            var values = Expression.Parameter(typeof(ObjectBase));

            var cast = new [] { Expression.Assign(mock, Expression.Convert(input, constructor.DeclaringType)) as Expression };

            var setters = type.SettableProperties.Where(p => p.SetMethod.IsPublic).Select(p => new { p.Name, type = p.PropertyType })
                .Concat(type.SettableFields.Where(f => f.IsPublic).Select(f => new { f.Name, type = f.FieldType }))
                .Select(p => Expression.IfThen(
                    // if (values.HasMockedFieldOrProperty<T>(name))
                    Expression.Call(values, HasMockedFieldOrProperty.MakeGenericMethod(new[] { p.type }), Expression.Constant(p.Name)),
                    // mock.name = values.GetProperty<T>(name);
                    Expression.Assign(Expression.PropertyOrField(mock, p.Name), Expression.Call(values, GetProperty.MakeGenericMethod(new[] { p.type }), Expression.Constant(p.Name)))));

            Setter = Expression.Lambda<Action<object, ObjectBase>>(
                Expression.Block(new[]{ mock }, cast.Concat(setters)), input, values).Compile();
        }

        public override object TryConstruct(ObjectBase objectBase, IEnumerable<object> otherArgs)
        {
            var constructed = base.TryConstruct(null, otherArgs);
            if (constructed != null)
                Setter(constructed, objectBase);

            return constructed;
        }
    }
}
