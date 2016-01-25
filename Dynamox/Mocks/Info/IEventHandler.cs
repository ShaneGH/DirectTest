using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks.Info
{
    public interface IEventHandler
    {
        bool CanBeInvokedWitTypes(IEnumerable<Type> withArgTypes);
        bool CanBeInvoked(IEnumerable<object> withArgs);
        void Invoke(IEnumerable<object> withArgs);
    }
}
