using System;
using System.Collections.Generic;
using System.Linq;

namespace Dynamox.Mocks
{
    public interface IMethodCallback
    {
        /// <summary>
        /// Invoke a callback if the argumets are valid
        /// </summary>
        /// <param name="args">The callback arguments</param>
        /// <returns>If arguments are valid</returns>
        bool Do(IEnumerable<object> args);
    }

    internal abstract class MethodCallbackBase : IMethodCallback
    {
        public virtual IEnumerable<Type> InputTypes
        {
            get
            {
                return Enumerable.Empty<Type>();
            }
        }

        public bool Do(IEnumerable<object> args)
        {
            var args1 = InputTypes.ToArray();
            var args2 = args.ToArray();

            if (args1.Length > args2.Length)
                return false;

            for (var i = 0; i < args1.Length; i++)
            {
                if (args1[i].IsValueType && args2[i] == null)
                    return false;

                if (args2[i] != null && !args1[i].IsAssignableFrom(args2[i].GetType()))
                    return false;
            }

            _Do(args);
            return true;
        }

        protected abstract void _Do(IEnumerable<object> args);
    }
}
