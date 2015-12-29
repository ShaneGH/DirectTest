using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    /// <summary>
    /// Wrap a ConstructorInfo object and attempts to call
    /// </summary>
    public class Constructor : IConstructor
    {
        readonly IReadOnlyCollection<Type> Parameters;
        readonly Func<object[], object> _Constructor;

        public Constructor(ConstructorInfo constructor)
        {
            Parameters = Array.AsReadOnly(constructor.GetParameters().Select(p => p.ParameterType).ToArray());

            var inputs = Expression.Parameter(typeof(object[]));
            var create = Expression.New(constructor,
                    Parameters.Select((p, i) => Expression.Convert(Expression.ArrayIndex(inputs, Expression.Constant(i)), p)));

            // compile the ConstructorInfo into a function
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
}
