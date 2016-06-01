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

        readonly object _lock = new object();

        MethodMockBuilder _MethodMock;
        MethodMockBuilder MethodMock
        {
            get
            {
                if (_MethodMock == null)
                {
                    lock (_lock)
                    {
                        if (!(FinalMockInstance is MockBuilder))
                        {
                            throw new InvalidOperationException("Invalid mock expression");
                        }

                        _MethodMock = (FinalMockInstance as MockBuilder)
                            .MockMethod(FinalMockExpression.Method.Name, FinalMockExpression.Method.GetGenericArguments(),
                            MockBuilder<TMockType>.GetValuesFromExpressions(FinalMockExpression.Arguments));
                    }
                }

                return _MethodMock;
            }
        }

        void MethodSetter(object value)
        {
            lock (_lock)
            {
                MethodMock.ReturnValue = value;
            }
        }

        void MethodEnsurer()
        {
            lock (_lock)
            {
                MethodMock.Ensure();
            }
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
            MethodSetter(value);
            return this;
        }

        IMockOrReturns<TMockType, TReturnType> IReturns<TMockType, TReturnType>.DxEnsure()
        {
            MethodEnsurer();
            return this;
        }
        dynamic IMockBuilder<TMockType>.WeakMock
        {
            get
            {
                return RootMock.WeakMock;
            }
        }

        dynamic IReturns<TMockType, TReturnType>.Weak
        {
            get
            {
                MethodMock.ReservedTerms.Set(new ReservedTerms());
                return MethodMock;
            }
        }
    }
}
