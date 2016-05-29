using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks.Info
{
    /// <summary>
    /// Marks a class as ensurable, so that Dx.Ensure(...) can determine what methods were not called.
    /// </summary>
    public interface IEnsure
    {
        /// <summary>
        /// Gets a list of messages, each one detailing a method which was marked to be called but was not
        /// </summary>
        IEnumerable<string> ShouldHaveBeenCalled { get; }
    }
}
