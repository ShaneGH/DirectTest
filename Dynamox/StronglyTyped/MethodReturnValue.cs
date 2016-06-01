using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks.Info;

namespace Dynamox.StronglyTyped
{
    class MethodReturnValue<TMockType, TReturnType> : IMockOrReturns<TMockType, TReturnType>
    {
        readonly MethodCallExpression FinalMockExpression;
        readonly MockBuilder<TMockType> RootMock;
        readonly object FinalMockInstance;

        public MethodReturnValue(MockBuilder<TMockType> rootMock, MethodCallExpression finalMockExpression, object finalMockInstance)
        {
            FinalMockExpression = finalMockExpression;
            RootMock = rootMock;
            FinalMockInstance = finalMockInstance;
        }

        IReturns<TMockType, TReturnType2> IMockBuilder<TMockType>.Mock<TReturnType2>(Expression<Func<TMockType, TReturnType2>> mockExpression)
        {
            return RootMock.Mock(mockExpression);
        }

        IReturns<TMockType, object> IMockBuilder<TMockType>.Mock(Expression<Action<TMockType>> mockExpression)
        {
            return RootMock.Mock(mockExpression);
        }

        TMockType IMockBuilder<TMockType>.DxAs()
        {
            return RootMock.DxAs();
        }

        IMockOrReturns<TMockType, TReturnType> IReturns<TMockType, TReturnType>.DxReturns(TReturnType value)
        {
            MethodSetter(FinalMockInstance, value);
            return this;
        }

        IMockOrReturns<TMockType, TReturnType> IReturns<TMockType, TReturnType>.DxEnsure()
        {
            MethodEnsurer(FinalMockInstance);
            return this;
        }

        readonly object _lock = new object();

        MethodMockBuilder MethodMock;
        MethodMockBuilder CreateMethodMock(object mockBuilder)
        {
            lock (_lock)
            {
                if (!(mockBuilder is MockBuilder))
                {
                    throw new InvalidOperationException("Invalid mock expression");
                }

                if (MethodMock == null)
                {
                    MethodMock = (mockBuilder as MockBuilder)
                        .MockMethod(FinalMockExpression.Method.Name, FinalMockExpression.Method.GetGenericArguments(),
                        MockBuilder<TMockType>.GetValuesFromExpressions(FinalMockExpression.Arguments));
                }

                return MethodMock;
            }
        }

        void MethodSetter(object setValueOf, object value)
        {
            lock (_lock)
            {
                CreateMethodMock(setValueOf).ReturnValue = value;
            }
        }

        void MethodEnsurer(object ensureValueOf)
        {
            lock (_lock)
            {
                CreateMethodMock(ensureValueOf).Ensure();
            }
        }
    }
}
