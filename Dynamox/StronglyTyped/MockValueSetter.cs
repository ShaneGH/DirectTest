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

    class MockValueSetter<TMock, TReturnType>
    {
        readonly Expression MockExpression;
        readonly object _lock = new object();

        public Expression LastGetter
        {
            get
            {
                if (MockExpression is MemberExpression)
                    return (MockExpression as MemberExpression).Expression;
                if (MockExpression is MethodCallExpression)
                    return (MockExpression as MethodCallExpression).Object;

                return null;
            }
        }

        public MockValueSetter(Expression mockExpression)
        {
            MockExpression = mockExpression;
        }

        public Returns<TMock, TReturnType> Returns(MockBuilder<TMock> mock, object finalGetterInstance)
        {
            var property = MockExpression as MemberExpression;
            var method = MockExpression as MethodCallExpression;

            if (property != null)
            {
                return new Returns<TMock, TReturnType>(mock, a => PropertySetter(finalGetterInstance, a), () => PropertyEnsurer(finalGetterInstance));
            }
            else if (method != null)
            {
                var asProperty = MockBuilder<TMock>.IsPropertyGetterOrSetter(method.Method);
                if (asProperty != null)
                {
                    return new Returns<TMock, TReturnType>(mock, a => IndexedPropertySetter(asProperty, finalGetterInstance, a), () => IndexedPropertyEnsurer(asProperty, finalGetterInstance));
                }
                else
                {
                    return new Returns<TMock, TReturnType>(mock, a => MethodSetter(finalGetterInstance, a), () => MethodEnsurer(finalGetterInstance));
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid mock expression");
            }
        }

        bool PropertyEnsure;
        bool PropertySet;
        object PropertyValue;

        void PropertySetter(object setValueOf, object value)
        {
            lock (_lock)
            {
                var property = MockExpression as MemberExpression;
                PropertyValue = value;
                PropertySet = true;

                if (PropertyEnsure)
                    value = new EnsuredProperty(value)
                    {
                        IsEnsured = true
                    };

                if ((setValueOf is MockBuilder))
                {
                    var name = property.Member.Name;
                    (setValueOf as MockBuilder).SetMember(name, value);
                }
                else if (property.Member is PropertyInfo)
                {
                    (property.Member as PropertyInfo).GetSetMethod().Invoke(setValueOf, new object[] { value });
                }
                else if (property.Member is FieldInfo)
                {
                    (property.Member as FieldInfo).SetValue(setValueOf, value);
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

        void IndexedPropertySetter(PropertyInfo property, object setValueOf, object value)
        {
            lock (_lock)
            {
                var method = MockExpression as MethodCallExpression;
                PropertyValue = value;
                PropertySet = true;

                if (PropertyEnsure)
                    value = new EnsuredProperty(value)
                    {
                        IsEnsured = true
                    };

                if ((setValueOf is MockBuilder))
                {
                    (setValueOf as MockBuilder).SetIndex(MockBuilder<TMock>.GetValuesFromExpressions(method.Arguments), value);
                }
                else
                {
                    property.GetSetMethod().Invoke(setValueOf, new object[] { value });
                }
            }
        }

        void IndexedPropertyEnsurer(PropertyInfo property, object setValueOf)
        {
            lock (_lock)
            {
                PropertyEnsure = true;
                if (PropertySet)
                    IndexedPropertySetter(property, setValueOf, PropertyValue);
            }
        }

        MethodMockBuilder MethodMock;
        MethodMockBuilder CreateMethodMock(object mockBuilder)
        {
            var method = MockExpression as MethodCallExpression;
            lock (_lock)
            {
                if (!(mockBuilder is MockBuilder))
                {
                    throw new InvalidOperationException("Invalid mock expression");
                }

                if (MethodMock == null)
                {
                    MethodMock = (mockBuilder as MockBuilder)
                        .MockMethod(method.Method.Name, method.Method.GetGenericArguments(), MockBuilder<TMock>.GetValuesFromExpressions(method.Arguments));
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