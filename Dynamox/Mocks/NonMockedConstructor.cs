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

    /// <summary>
    /// Wrap a ConstructorInfo object and attempts to call. If successful, will set all of the public properties of that class which have been mocked
    /// </summary>
    public class NonMockedConstructor : Constructor
    {
        static readonly MethodInfo HasMockedFieldOrProperty = ObjectBase.Reflection.HasMockedFieldOrProperty;
        static readonly MethodInfo GetProperty = ObjectBase.Reflection.GetProperty;

        readonly Action<object, ObjectBase> Setter;

        public NonMockedConstructor(ConstructorInfo constructor)
            : base(constructor)
        {
            var type = TypeOverrideDescriptor.Create(constructor.DeclaringType);
            var mock = Expression.Variable(constructor.DeclaringType);
            var input = Expression.Parameter(typeof(object));
            var values = Expression.Parameter(typeof(ObjectBase));

            var cast = new[] { Expression.Assign(mock, Expression.Convert(input, constructor.DeclaringType)) as Expression };

            var setters = type.SettableProperties.Where(p => p.SetMethod.IsPublic).Select(p => new { p.Name, type = p.PropertyType })
                .Concat(type.SettableFields.Where(f => f.IsPublic).Select(f => new { f.Name, type = f.FieldType }))
                .Select(p => Expression.IfThen(
                    // if (values.HasMockedFieldOrProperty<T>(name))
                    Expression.Call(values, HasMockedFieldOrProperty.MakeGenericMethod(new[] { p.type }), Expression.Constant(p.Name)),
                    // mock.name = values.GetProperty<T>(name, false);
                    Expression.Assign(Expression.PropertyOrField(mock, p.Name), Expression.Call(values, GetProperty.MakeGenericMethod(new[] { p.type }), Expression.Constant(p.Name), Expression.Constant(false)))));

            Setter = Expression.Lambda<Action<object, ObjectBase>>(
                Expression.Block(new[] { mock }, cast.Concat(setters)), input, values).Compile();
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
