using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests.Builders
{
    public partial class TestBuilder
    {
        private class Asserter<TTestResult> : IAssert<TTestResult>
        {
            private readonly TestBuilder BasedOn;

            public Asserter(TestBuilder basedOn)
            {
                BasedOn = basedOn;
            }

            public ITest Assert(Action<dynamic, TTestResult> result)
            {
                //TODO, catch cast errors
                BasedOn._Assert.Add((a, b) => result(a, (TTestResult)b));
                return this;
            }

            public ITest Assert(Action<dynamic> result)
            {
                BasedOn.Assert(result);
                return this;
            }

            public ITest Throws<TException>(Action<dynamic, TException> result) where TException : Exception
            {
                BasedOn.Throws<TException>(result);
                return this;
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

            public void Run() 
            {
                Framework.Run(this);
            }

            public TestBuilder Builder
            {
                get { return BasedOn; }
            }
        }
    }
}
