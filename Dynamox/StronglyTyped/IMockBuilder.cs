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
    /// <summary>
    /// A stongly typed mock builder
    /// </summary>
    /// <typeparam name="T">The type of the mock</typeparam>
    public interface IMockBuilder<T>
    {
        /// <summary>
        /// Mock a new method, property or chain
        /// </summary>
        /// <typeparam name="TReturnType">The type of the method or property</typeparam>
        /// <param name="mockExpression">An expression describing the method or property</param>
        /// <returns>An object to modify the return value</returns>
        IReturns<T, TReturnType> Mock<TReturnType>(Expression<Func<T, TReturnType>> mockExpression);

        /// <summary>
        /// Mock a new method, property or chain
        /// </summary>
        /// <param name="mockExpression">An expression describing the method or property</param>
        /// <returns>An object to modify the return value</returns>
        IReturns<T, object> Mock(Expression<Action<T>> mockExpression);

        /// <summary>
        /// Get this mock object as a weakly typed mock
        /// </summary>
        dynamic WeakMock { get; }

        /// <summary>
        /// Get the object instance associated with this class
        /// </summary>
        /// <returns></returns>
        T DxAs();
    }

    /// <summary>
    /// Modify the return value of a mocked expression
    /// </summary>
    /// <typeparam name="TMock">The type of the underlying mock</typeparam>
    /// <typeparam name="TReturnType">The type to return</typeparam>
    public interface IReturns<TMock, TReturnType>
    {
        /// <summary>
        /// Get the return value of this method or property as a weakly typed mock
        /// </summary>
        dynamic Weak { get; }

        /// <summary>
        /// Set the value to return
        /// </summary>
        IMockOrReturns<TMock, TReturnType> DxReturns(TReturnType value);

        /// <summary>
        /// Ensure that this method is called
        /// </summary>
        IMockOrReturns<TMock, TReturnType> DxEnsure();
    }

    public interface IMockOrReturns<TMock, TReturnType> : 
        IMockBuilder<TMock>, IReturns<TMock, TReturnType>
    {
    }
}
