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
    public class MockBuilder<T>
    {
        internal readonly Mocks.Info.MockBuilder _mock;
        readonly List<Expression<Func<T, bool>>> MockExpressions = new List<Expression<Func<T, bool>>>();

        // use unique names for all reserved terms
        static readonly ReservedTerms DefaultReservedTerms = new ReservedTerms
        {
            DxAs = Guid.NewGuid().ToString(),
            DxClear = Guid.NewGuid().ToString(),
            DxConstructor = Guid.NewGuid().ToString(),
            DxDo = Guid.NewGuid().ToString(),
            DxEnsure = Guid.NewGuid().ToString(),
            DxOut = Guid.NewGuid().ToString(),
            DxReturns = Guid.NewGuid().ToString()
        };

        static readonly PropertyInfo DxAny = typeof(Dx)
            .GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Where(m => m.Name == "Any")
            .First();


        static readonly MethodInfo DxAnyT = typeof(Dx)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Where(m => m.Name == "AnyT")
            .First();

        static bool IsDxAny(MethodInfo method)
        {
            return method.IsGenericMethod && method.GetGenericMethodDefinition() == DxAnyT;
        }

        static bool IsDxAny(MemberInfo property)
        {
            return property == DxAny;
        }

        static PropertyInfo IsPropertyGetterOrSetter(MethodInfo method)
        {
            return method.DeclaringType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(p => p.GetAccessors(true).Contains(method));
        }

        public MockBuilder(IEnumerable<object> constructorArgs = null) 
        {
            _mock = CreateMockBuilder(constructorArgs);
        }

        public Returns<T, object> Mock(Expression<Action<T>> mockExpression)
        {
            if (mockExpression == null)
                throw new InvalidOperationException("Invalid mock expression");

            return _Mock<object>(mockExpression.Body, mockExpression.Parameters[0]);
        }

        public Returns<T, TReturnType> Mock<TReturnType>(Expression<Func<T, TReturnType>> mockExpression)
        {
            if (mockExpression == null)
                throw new InvalidOperationException("Invalid mock expression");

            return _Mock<TReturnType>(mockExpression.Body, mockExpression.Parameters[0]);
        }

        Returns<T, TReturnType> _Mock<TReturnType>(Expression mockExpression, ParameterExpression rootObject)
        {
            if (mockExpression == null)
                throw new InvalidOperationException("Invalid mock expression");

            try
            {

                _mock.MockSettings.Set(DefaultReservedTerms);
                Expression current = null;
                Action<object, object> setter = null;

                var property = mockExpression as MemberExpression;
                var method = mockExpression as MethodCallExpression;

                if (property != null)
                {
                    setter = (setValueOf, value) =>
                    {
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
                    };

                    current = property.Expression;
                }
                else if (method != null)
                {
                    var args = method.Arguments.Select(a =>
                    {
                        if (a.NodeType == ExpressionType.Convert || a.NodeType == ExpressionType.ConvertChecked)
                            a = (a as UnaryExpression).Operand;

                        if (a is ConstantExpression)
                            return (a as ConstantExpression).Value;

                        if ((a is MethodCallExpression && IsDxAny((a as MethodCallExpression).Method)) ||
                            (a is MemberExpression && IsDxAny((a as MemberExpression).Member)))
                            return Dx.Any;

                        throw new InvalidOperationException("Invalid mock expression");
                    });

                    var asProperty = IsPropertyGetterOrSetter(method.Method);
                    if (asProperty != null)
                    {
                        setter = (setValueOf, value) =>
                        {
                            if ((setValueOf is MockBuilder))
                            {
                                (setValueOf as MockBuilder).SetIndex(args, value);
                            }
                            else
                            {
                                asProperty.GetSetMethod().Invoke(setValueOf, new object[] { value });
                            }
                        };
                    }
                    else
                    {
                        setter = (setValueOf, value) =>
                        {
                            if (!(setValueOf is MockBuilder))
                            {
                                throw new InvalidOperationException("Invalid mock expression");
                            }

                            (setValueOf as MockBuilder)
                                .MockMethod(method.Method.Name, method.Method.GetGenericArguments(), args)
                                .ReturnValue = value;
                        };
                    }

                    current = method.Object;
                }
                else
                {
                    throw new InvalidOperationException("Invalid mock expression");
                }

                var getters = new List<Expression>();
                while (true)
                {
                    if (current == rootObject)
                    {
                        break;
                    }
                    else if (current is MemberExpression)
                    {
                        getters.Insert(0, current);
                        current = (current as MemberExpression).Expression;
                    }
                    else if (current is MethodCallExpression)
                    {
                        getters.Insert(0, current);
                        current = (current as MethodCallExpression).Object;
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid mock expression");
                    }
                }

                object c = _mock, val;
                while (getters.Any())
                {
                    current = getters[0];
                    getters.RemoveAt(0);

                    if (current is MemberExpression)
                    {
                        if ((c is MockBuilder))
                        {
                            var name = (current as MemberExpression).Member.Name;
                            if (!(c as MockBuilder).TryGetMember(name, out val))
                            {
                                (c as MockBuilder).SetMember(name, val = CreateMockBuilder(reservedTerms: DefaultReservedTerms));
                            }

                            c = val;
                        }
                        else if ((current as MemberExpression).Member is PropertyInfo)
                        {
                            c = ((current as MemberExpression).Member as PropertyInfo).GetGetMethod().Invoke(c, new object[0]);
                        }
                        else if ((current as MemberExpression).Member is FieldInfo)
                        {
                            c = ((current as MemberExpression).Member as FieldInfo).GetValue(c);
                        }
                        else
                        {
                            throw new InvalidOperationException("Invalid mock expression");
                        }
                    }
                    else if (current is MethodCallExpression)
                    {
                        var args = (current as MethodCallExpression).Arguments.Select(a =>
                        {
                            if (a.NodeType == ExpressionType.Convert || a.NodeType == ExpressionType.ConvertChecked)
                                a = (a as UnaryExpression).Operand;

                            if (a is ConstantExpression)
                                return (a as ConstantExpression).Value;

                            if (a is MemberExpression && IsDxAny((a as MemberExpression).Member))
                                return Dx.Any;

                            if (a is MethodCallExpression && IsDxAny((a as MethodCallExpression).Method))
                                return new AnyValue((a as MethodCallExpression).Method.GetGenericArguments()[0]);

                            throw new InvalidOperationException("Invalid mock expression");
                        });

                        if (c is MockBuilder)
                        {
                            var asProperty = IsPropertyGetterOrSetter((current as MethodCallExpression).Method);
                            if (asProperty != null)
                            {
                                var tmp = CreateMockBuilder(reservedTerms: DefaultReservedTerms);
                                (c as MockBuilder).SetIndex(args, tmp);
                                c = tmp;
                            }
                            else
                            {
                                var name = (current as MethodCallExpression).Method.Name;
                                c = (c as MockBuilder)
                                    .MockMethod(name, (current as MethodCallExpression).Method.GetGenericArguments(), args)
                                    .ReturnValue = CreateMockBuilder(reservedTerms: DefaultReservedTerms);
                            }
                        }
                        else
                        {
                            c = (current as MethodCallExpression).Method.Invoke(c, args.ToArray());
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid mock expression");
                    }
                }

                return new Returns<T, TReturnType>(this, a => setter(c, a));
            }
            finally
            {
                _mock.MockSettings.Set(ReservedTerms.Default);
            }
        }

        static MockBuilder CreateMockBuilder(IEnumerable<object> constructorArgs = null, IReservedTerms reservedTerms = null)
        {
            return new MockBuilder(reservedTerms ?? ReservedTerms.Default, DxSettings.GlobalSettings, constructorArgs);
        }

        public T Build() 
        {
            return (T)_mock.Mock(typeof(T));
        }

        public class Returns<TMockType, TReturnType>
        {
            Action<TReturnType> Setter;
            MockBuilder<TMockType> Builder;

            public Returns(MockBuilder<TMockType> builder, Action<TReturnType> setter)
            {
                Setter = setter;
                Builder = builder;
            }

            public MockBuilder<TMockType> DxReturns(TReturnType value) 
            {
                Setter(value);
                return Builder;
            }
        }
    }
}
