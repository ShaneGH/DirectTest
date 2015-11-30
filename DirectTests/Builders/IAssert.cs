using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests.Builders
{
    public interface IAssert : ITest
    {
        IAssert SkipParentAssert(bool skipParentAssert = true);
        IAssert SkipParentThrows(bool skipParentThrows = true);
        ITest Assert(Action<dynamic> result);
        ITest Throws<TException>() where TException : Exception;
        ITest Throws<TException>(Action<dynamic, TException> result) where TException : Exception;
    }

    public interface IAssert<TResult> : IAssert
    {
        new IAssert<TResult> SkipParentThrows(bool skipParentThrows = true);
        new IAssert<TResult> SkipParentAssert(bool skipParentAssert = true);
        ITest Assert(Action<dynamic, TResult> result);
        ITest Assert<TExpectedResult>(Action<dynamic, TExpectedResult> result);
    }
}