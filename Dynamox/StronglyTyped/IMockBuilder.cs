using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;
using Dynamox.Mocks.Info;

namespace Dynamox.StronglyTyped
{
    public interface IMockBuilder<T>
    {
        IReturns<T, TReturnType> Mock<TReturnType>(Expression<Func<T, TReturnType>> mockExpression);

        IReturns<T, object> Mock(Expression<Action<T>> mockExpression);

        T DxAs();
    }

    public interface IReturns<TMock, TReturnType>
    {
        IMockOrReturns<TMock, TReturnType> DxReturns(TReturnType value);

        IMockOrReturns<TMock, TReturnType> DxEnsure();
    }

    public interface IMockOrReturns<TMock, TReturnType> : 
        IMockBuilder<TMock>, IReturns<TMock, TReturnType>
    {
    }
}
