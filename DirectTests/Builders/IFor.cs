using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests.Builders
{
    public interface IFor : ITest
    {
        IParameterizedArrange<object> For(MethodInfo method);
    }

    public interface IFor<TSubject> : ITest
    {
        IParameterizedArrange<TReturnValue> For<TReturnValue>(Expression<Func<TSubject, TReturnValue>> act);

        IParameterizedArrange For(Expression<Action<TSubject>> act);
    }
}
