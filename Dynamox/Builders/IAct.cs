using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Builders
{
    public interface IAct : ITest
    {
        IActAssert UseParentAct(bool useParentAct = true);
        IAssert<TActResult> UseParentAct<TActResult>(bool useParentAct = true);
        IAssert Act(Action<dynamic> action);
        IAssert<TResult> Act<TResult>(Func<dynamic, TResult> action);
    }

    public interface IActAssert : IAct, IAssert { }
}