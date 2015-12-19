using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Builders
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

            public ITest Assert<TExpectedResult>(Action<dynamic, TExpectedResult> result)
            {
                BasedOn._Assert.Add((a, b) =>
                {
                    if ((typeof(TExpectedResult).IsValueType && b == null) || (b != null && !(b is TExpectedResult)))
                        throw new InvalidOperationException("Invalid return type"); //TODE

                    result(a, (TExpectedResult)b);
                });
                return this;
            }

            public ITest Assert(Action<dynamic, TTestResult> result)
            {
                return Assert<TTestResult>(result);
            }

            public ITest Assert(Action<dynamic> result)
            {
                BasedOn.Assert(result);
                return this;
            }

            public ITest Throws<TException>(Action<dynamic, TException> result)
                where TException : Exception
            {
                BasedOn.Throws<TException>(result);
                return this;
            }

            public ITest Throws<TException>()
                where TException : Exception
            {
                return Throws<TException>((a, b) => { });
            }

            public IAssert<TTestResult> SkipParentThrows(bool skipParentThrows = true)
            {
                BasedOn._UseBaseThrows = !skipParentThrows;
                return this;
            }

            public IAssert<TTestResult> SkipParentAssert(bool skipParentAssert = true)
            {
                BasedOn._UseBaseAssert = !skipParentAssert;
                return this;
            }

            IAssert IAssert.SkipParentThrows(bool skipParentThrows = true)
            {
                return BasedOn.SkipParentThrows(skipParentThrows);
            }

            IAssert IAssert.SkipParentAssert(bool skipParentAssert)
            {
                return BasedOn.SkipParentAssert(skipParentAssert);
            }

            public void Run() 
            {
                Dx.Run(this);
            }

            public TestBuilder Builder
            {
                get { return BasedOn; }
            }
        }
    }
}
