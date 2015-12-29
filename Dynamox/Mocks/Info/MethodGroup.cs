using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks.Info
{
    /// <summary>
    /// Represents a group of MethodMockBuilders which all have the same name and will be invoked as a group
    /// </summary>
    internal class MethodGroup : Collection<MethodMockBuilder>
    {
        public MethodGroup()
            : base()
        {
        }

        public MethodGroup(MethodMockBuilder first)
            : this()
        {
            Add(first);
        }

        /// <summary>
        /// Try to invoke a method in the group based on on the arguments given. Will return after the first success result
        /// </summary>
        /// <param name="genericArguments"></param>
        /// <param name="arguments"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryInvoke(IEnumerable<Type> genericArguments, IEnumerable<MethodArg> arguments, out object result)
        {
            foreach (var item in this)
                if (item.TryInvoke(genericArguments, arguments, out result))
                    return true;

            result = null;
            return false;
        }

        /// <summary>
        /// Returns a a summary of the args of any method which was strict mocked and not called
        /// </summary>
        public IEnumerable<string> ShouldHaveBeenCalled
        {
            get
            {
                return this.Where(m => m.MustBeCalled && !m.WasCalled)
                    .Select(m => m.ArgChecker.InputTypes.Any() ? "Args: " + string.Join(", ", m.ArgChecker.InputTypes) : string.Empty)
                    .Union(
                        this.Select(m => m.ReturnValue).OfType<MockBuilder>().SelectMany(b => b.ShouldHaveBeenCalled));
            }
        }
    }
}
