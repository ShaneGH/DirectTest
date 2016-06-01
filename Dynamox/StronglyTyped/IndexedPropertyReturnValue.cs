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
    class IndexedPropertyReturnValue<TMockType, TReturnType> : IMockOrReturns<TMockType, TReturnType>
    {
        readonly MethodCallExpression FinalMockExpression;
        readonly MockBuilder<TMockType> RootMock;
        readonly object FinalMockInstance;
        readonly PropertyInfo IndexedProperty;

        public IndexedPropertyReturnValue(MockBuilder<TMockType> rootMock, PropertyInfo indexedProperty, MethodCallExpression finalMockExpression, object finalMockInstance)
        {
            FinalMockExpression = finalMockExpression;
            RootMock = rootMock;
            FinalMockInstance = finalMockInstance;
            IndexedProperty = indexedProperty;
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
            PropertySetter(FinalMockInstance, value);
            return this;
        }

        IMockOrReturns<TMockType, TReturnType> IReturns<TMockType, TReturnType>.DxEnsure()
        {
            PropertyEnsurer(FinalMockInstance);
            return this;
        }

        bool PropertyEnsure;
        bool PropertySet;
        object PropertyValue;
        readonly object _lock = new object();

        void PropertySetter(object setValueOf, object value)
        {
            lock (_lock)
            {
                PropertyValue = value;
                PropertySet = true;

                if (PropertyEnsure)
                    value = new EnsuredProperty(value)
                    {
                        IsEnsured = true
                    };

                if ((setValueOf is MockBuilder))
                {
                    (setValueOf as MockBuilder).SetIndex(MockBuilder<TMockType>.GetValuesFromExpressions(FinalMockExpression.Arguments), value);
                }
                else
                {
                    IndexedProperty.GetSetMethod().Invoke(setValueOf, new[] { value });
                }
            }
        }

        void PropertyEnsurer(object setValueOf)
        {
            lock (_lock)
            {
                PropertyEnsure = true;
                if (PropertySet)
                    PropertySetter(setValueOf, PropertyValue);
            }
        }
    }
}
