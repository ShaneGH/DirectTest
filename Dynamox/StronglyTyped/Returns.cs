using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks.Info;

namespace Dynamox.StronglyTyped
{
    internal class Returns<TMockType, TReturnType> : IMockOrReturns<TMockType, TReturnType>
    {
        Action<object> Setter;
        Action Ensurer;
        MockBuilder<TMockType> Builder;

        public Returns(MockBuilder<TMockType> builder, Action<object> setter, Action ensurer)
        {
            Setter = setter;
            Ensurer = ensurer;
            Builder = builder;
        }

        public IMockOrReturns<TMockType, TReturnType> DxEnsure()
        {
            if (Ensurer == null)
                throw new InvalidOperationException("You cannot ensure this mocked item");

            Ensurer();
            return this;
        }

        public IMockOrReturns<TMockType, TReturnType> DxReturns(TReturnType value)
        {
            Setter(value);
            return this;
        }

        public IReturns<TMockType, object> Mock(Expression<Action<TMockType>> mockExpression)
        {
            return Builder.Mock(mockExpression);
        }

        public IReturns<TMockType, TNewReturnType> Mock<TNewReturnType>(Expression<Func<TMockType, TNewReturnType>> mockExpression)
        {
            return Builder.Mock(mockExpression);
        }

        public TMockType DxAs()
        {
            return Builder.DxAs();
        }

        dynamic IMockBuilder<TMockType>.WeakMock
        {
            get 
            {
                return Builder.WeakMock;
            }
        }

        dynamic IReturns<TMockType, TReturnType>.Weak
        {
            get 
            {
                var returns = new MockBuilder();
                Setter(returns);
                return returns;
            }
        }
    }
}
