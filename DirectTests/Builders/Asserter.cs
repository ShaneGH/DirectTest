using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests.Builders
{
    public partial class TestBuilder : IBasedOn, IArrange, IAct, IAssert
    {
        private class Asserter<TTestResult> : IAssert<TTestResult>
        {
            private readonly TestBuilder BasedOn;

            public Asserter(TestBuilder basedOn)
            {
                BasedOn = basedOn;
            }

            public void Assert(Action<dynamic, TTestResult> result)
            {
                //TODO, catch cast errors
                BasedOn._Assert.Add((a, b) => result(a, (TTestResult)b));
            }

            public void Assert(Action<dynamic> result)
            {
                BasedOn.Assert(result);
            }

            public void Throws<TException>(Action<dynamic, TException> result) where TException : Exception
            {
                BasedOn.Throws<TException>(result);
            }

            public IAssert<TTestResult> SkipParentAssert(bool skipParentAssert = true)
            {
                BasedOn._UseBaseAssert = !skipParentAssert;
                return this;
            }

            IAssert IAssert.SkipParentAssert(bool skipParentAssert)
            {
                return BasedOn.SkipParentAssert(skipParentAssert);
            }
        }
    }
}
