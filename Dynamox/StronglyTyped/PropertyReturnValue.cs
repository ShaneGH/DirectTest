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
    class PropertyReturnValue<TMockType, TReturnType> : IMockOrReturns<TMockType, TReturnType>
    {
        readonly MemberExpression FinalMockExpression;
        readonly MockBuilder<TMockType> RootMock;
        readonly object FinalMockInstance;

        public PropertyReturnValue(MockBuilder<TMockType> rootMock, MemberExpression finalMockExpression, object finalMockInstance)
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
                    var name = FinalMockExpression.Member.Name;
                    (setValueOf as MockBuilder).SetMember(name, value);
                }
                else if (FinalMockExpression.Member is PropertyInfo)
                {
                    (FinalMockExpression.Member as PropertyInfo).GetSetMethod().Invoke(setValueOf, new object[] { value });
                }
                else if (FinalMockExpression.Member is FieldInfo)
                {
                    (FinalMockExpression.Member as FieldInfo).SetValue(setValueOf, value);
                }
                else
                {
                    throw new InvalidOperationException("Invalid mock expression");
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
