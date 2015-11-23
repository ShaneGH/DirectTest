using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests.Builders
{
    public interface IAct : ITest
    {
        IAct UseParentAct(bool useParentAct = true);
        IAssert Act(Action<dynamic> action);
        IAssert<TResult> Act<TResult>(Func<dynamic, TResult> action);
    }
}