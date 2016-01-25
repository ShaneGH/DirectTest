using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Compile
{
    /// <summary>
    /// Expose event raising functionality to external objects
    /// </summary>
    public interface IRaiseEvent
    {
        bool RaiseEvent(string eventName, object[] args);
    }
}
