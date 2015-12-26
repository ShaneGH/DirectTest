using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    public interface IMethodAssert
    {
        bool TestArgs(IEnumerable<object> args);
        
        // TODO: out params
        bool TestInputArgTypes(IEnumerable<Type> types);
        
        // TODO: out params
        bool CanMockMethod(MethodInfo method);

        IEnumerable<Type> InputTypes { get; }
    }

    internal class MethodApplicabilityChecker : IMethodAssert
    {
        public static readonly object Any = new AnyValue(typeof(AnyValue));

        public static object AnyT<T>()
        {
            return new AnyValue(typeof(T));
        }

        public virtual IEnumerable<Type> InputTypes
        {
            get
            {
                return Enumerable.Empty<Type>();
            }
        }

        public bool TestArgs()
        {
            return TestArgs(Enumerable.Empty<object>());
        }

        public bool TestInputArgTypes(IEnumerable<Type> types)
        {
            var methodArgTypes = InputTypes.ToArray();
            var inputArgTypes = types.ToArray();

            if (methodArgTypes.Length != inputArgTypes.Length)
                return false;

            for (var i = 0; i < methodArgTypes.Length; i++)
            {
                if (methodArgTypes[i] == typeof(AnyValue)) ;
                else if (!methodArgTypes[i].IsAssignableFrom(inputArgTypes[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanMockMethod(MethodInfo method)
        {
            var mockArgTypes = InputTypes.ToArray();
            var methodArgTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

            if (mockArgTypes.Length != methodArgTypes.Length)
                return false;

            for (var i = 0; i < mockArgTypes.Length; i++)
            {
                if (mockArgTypes[i] == typeof(AnyValue)) ;
                else if (methodArgTypes[i].IsGenericParameter)
                {
                    var genericConstraints = methodArgTypes[i].GetGenericParameterConstraints();
                    if (genericConstraints.Any() && !genericConstraints.Any(t => t.IsAssignableFrom(mockArgTypes[i])))
                        return false;
                }
                else if (!methodArgTypes[i].IsAssignableFrom(mockArgTypes[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool TestArgs(IEnumerable<object> args)
        {
            var methodArgs = InputTypes.ToArray();
            var inputArgs = args.ToArray();

            if (methodArgs.Length != inputArgs.Length)
                return false;

            for (var i = 0; i < methodArgs.Length; i++)
            {
                if (methodArgs[i] == typeof(AnyValue)) ;
                else if (inputArgs[i] == null)
                {
                    if (methodArgs[i].IsValueType)
                        return false;
                }
                else if (!methodArgs[i].IsAssignableFrom(inputArgs[i].GetType()))
                {
                    return false;
                }
            }

            return _TestArgs(args);
        }

        protected virtual bool _TestArgs(IEnumerable<object> args)
        {
            return !args.Any();
        }

        protected sealed class AnyValue 
        {
            public readonly Type OfType;

            public AnyValue(Type ofType) 
            {
                OfType = ofType;
            }
        }
    }
}
