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
                var returns = new MockBuilder();
               // Setter(returns);
                return returns;
            }
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

        object GetReturnValueAsWeakMock()
        {
            throw new NotImplementedException();
            //const string errorMessage =
            //    "You cannot convert an object which has been mocked as a concrete type into a weak mock";

            //var finalMock = FinalMockInstance as MockBuilder;
            //if (finalMock == null)
            //    throw new InvalidMockException(errorMessage);

            //object returns;
            //finalMock.TryGetIndex(FinalMockExpression.Member.Name, out returns);
            //if (!(returns is MockBuilder))
            //    throw new InvalidMockException(errorMessage);

            //return returns;
        }
    }
}
