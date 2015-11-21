using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests
{
    public interface IAssert
    {
        IAssert SkipParentAssert(bool skipParentAssert = true);
        void Assert(Action<dynamic> result);
        void Throws<TException>(Action<dynamic, TException> result)
            where TException : Exception;
    }

    public interface IAssert<TResult> : IAssert
    {
        new IAssert<TResult> SkipParentAssert(bool skipParentAssert = true);
        void Assert(Action<dynamic, TResult> result);
    }
}