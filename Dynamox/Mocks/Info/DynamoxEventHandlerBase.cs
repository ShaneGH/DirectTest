using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks.Info
{
    public abstract class DynamoxEventHandlerBase : IEventHandler
    {
        public abstract IEnumerable<Type> EventArgTypes { get; }

        public bool CanBeInvoked(IEnumerable<object> withArgs)
        {
            var args = withArgs.ToArray();
            var argTypes = EventArgTypes.ToArray();

            if (argTypes.Length > args.Length)
                return false;

            for (var i = 0; i < argTypes.Length; i++)
            {
                if (args[i] == null)
                {
                    if (argTypes[i].IsValueType)
                        return false;

                    continue;
                }

                if (!argTypes[i].IsAssignableFrom(args[i].GetType()))
                    return false;
            }

            return true;
        }

        public abstract void Invoke(IEnumerable<object> withArgs);
    }
}
