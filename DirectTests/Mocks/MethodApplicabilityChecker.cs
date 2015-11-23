using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests.Mocks
{
    public interface IMethodAssert
    {
        bool TestArgs(IEnumerable<object> args);
    }

    internal class MethodApplicabilityChecker : IMethodAssert
    {
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

        public bool TestArgs(IEnumerable<object> args)
        {
            var methodArgs = InputTypes.ToArray();
            var inputArgs = args.ToArray();

            if (methodArgs.Length != inputArgs.Length)
                return false;

            for (var i = 0; i < methodArgs.Length; i++)
            {
                if (methodArgs[i] == typeof(Any)) ;
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

        protected sealed class Any { }
    }
}
