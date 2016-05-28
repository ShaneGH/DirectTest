using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks.Info;

namespace Dynamox.Mocks
{
    /// <summary>
    /// Checks whether a mocked method can be used with a set of args or method
    /// </summary>
    internal class MethodApplicabilityChecker : IMethodAssert
    {
        public static readonly object Any = new AnyValue(typeof(AnyValue));
        public List<OutArg> OutParamValues { get; set; }

        public static AnyValue<T> AnyT<T>()
        {
            return new AnyValue<T>();
        }

        public virtual IEnumerable<Type> ArgTypes
        {
            get
            {
                return Enumerable.Empty<Type>();
            }
        }

        public MethodApplicabilityChecker() 
        {
            OutParamValues = new List<OutArg>();
        }

        public bool TestArgs()
        {
            return TestArgs(Enumerable.Empty<MethodArg>());
        }

        public bool TestInputArgTypes(IEnumerable<MethodArg> inputArgs)
        {
            var methodArgTypes = ArgTypes.ToArray();
            var inputArgTypes = inputArgs.Select(a => a.ArgType).ToArray();

            if (methodArgTypes.Length != inputArgTypes.Length)
                return false;

            object param;
            for (var i = 0; i < methodArgTypes.Length; i++)
            {
                if (typeof(AnyValue).IsAssignableFrom(methodArgTypes[i])) ;
                else if (!methodArgTypes[i].IsAssignableFrom(inputArgTypes[i]))
                {
                    return false;
                }

                if (OutParamValues.Any(p => p.Index == i))
                {
                    param = OutParamValues.First(p => p.Index == i).Value;
                    if ((param == null && inputArgTypes[i].IsValueType) ||
                        (param != null && !inputArgTypes[i].IsAssignableFrom(param.GetType())))
                    {
                        return false;
                    }
                }

                var ia = inputArgs.ElementAt(i);
                if (OutParamValues.Any(p => p.Name == ia.ArgName))
                {
                    param = OutParamValues.First(p => p.Name == ia.ArgName).Value;
                    if ((param == null && inputArgTypes[i].IsValueType) ||
                        (param != null && !inputArgTypes[i].IsAssignableFrom(param.GetType())))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool CanMockMethod(MethodInfo method)
        {
            var mockArgTypes = ArgTypes.ToArray();
            var methodArgTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

            if (mockArgTypes.Length != methodArgTypes.Length)
                return false;

            for (var i = 0; i < mockArgTypes.Length; i++)
            {
                if (typeof(AnyValue).IsAssignableFrom(mockArgTypes[i])) ;
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

        /// <summary>
        /// For testing purposes only
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Obsolete]
        internal bool TestArgs(IEnumerable<object> args)
        {
            return TestArgs(args.Select(a => new MethodArg(a, a == null ? typeof(object) : a.GetType(), "")));
        }

        public bool TestArgs(IEnumerable<MethodArg> args)
        {
            if (!TestInputArgTypes(args))
                return false;

            var methodArgs = ArgTypes.ToArray();
            var inputArgs = args.Select(a => a.Arg).ToArray();

            if (methodArgs.Length != inputArgs.Length)
                return false;

            for (var i = 0; i < methodArgs.Length; i++)
            {
                if (typeof(AnyValue).IsAssignableFrom(methodArgs[i])) ;
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

            return _TestArgs(args.Select(a => a.Arg));
        }

        protected virtual bool _TestArgs(IEnumerable<object> args)
        {
            return !args.Any();
        }
    }

        public class AnyValue
        {
            public readonly Type OfType;

            public AnyValue(Type ofType)
            {
                OfType = ofType;
            }

            public bool IsAnyValueType(object input) 
            {
                return OfType == typeof(AnyValue) || 
                    (input == null && !OfType.IsValueType) ||
                    (input != null && OfType.IsAssignableFrom(input.GetType()));
            }
        }

        public sealed class AnyValue<T> : AnyValue
        {
            public AnyValue()
                : base (typeof(T))
            {
            }

            public static implicit operator T(AnyValue<T> b) 
            {
                return default(T);
            }
        }
}
