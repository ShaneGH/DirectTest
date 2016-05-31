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
    public class MockBuilder<T> : IEnsure
    {
        internal readonly Mocks.Info.MockBuilder _mock;
        readonly List<Expression<Func<T, bool>>> MockExpressions = new List<Expression<Func<T, bool>>>();
        readonly DxSettings MockSettings;

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

        static readonly PropertyInfo DxAny = TypeUtils.GetProperty<object>(() => Dx.Any);
        static readonly MethodInfo DxAnyT1 = TypeUtils.GetMethod(() => Dx.AnyT<string>());
        static readonly MethodInfo DxAnyT2 = TypeUtils.GetMethod(() => Dx.AnyT(default(Type)));

        static bool IsDxAny(MethodInfo method)
        {
            return (method.IsGenericMethod && method.GetGenericMethodDefinition() == DxAnyT1) ||
                (!method.IsGenericMethod && method == DxAnyT2);
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
            : this(DxSettings.GlobalSettings, constructorArgs)
        {
        }

        public MockBuilder(DxSettings settings, IEnumerable<object> constructorArgs = null)
        {
            MockSettings = settings;
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
                Action<object> ensurer = null;

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
                        var _lock = new object();
                        MethodMockBuilder methodMock = null;
                        Func<object, MethodMockBuilder> creator = mockBuilder =>
                        {
                            if (!(mockBuilder is MockBuilder))
                            {
                                throw new InvalidOperationException("Invalid mock expression");
                            }

                            lock (_lock)
                            {
                                if (methodMock == null)
                                {
                                    methodMock = (mockBuilder as MockBuilder)
                                        .MockMethod(method.Method.Name, method.Method.GetGenericArguments(), args);
                                }
                            }

                            return methodMock;
                        };

                        setter = (setValueOf, value) => creator(setValueOf).ReturnValue = value;
                        ensurer = ensureValueOf => creator(ensureValueOf).Ensure();
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

                Action _ensurer = null;
                if (ensurer != null) _ensurer = () => ensurer(c);
                return new Returns<T, TReturnType>(this, a => setter(c, a), _ensurer);
            }
            finally
            {
                _mock.MockSettings.Set(ReservedTerms.Default);
            }
        }

        MockBuilder CreateMockBuilder(IEnumerable<object> constructorArgs = null, IReservedTerms reservedTerms = null)
        {
            return new MockBuilder(reservedTerms ?? ReservedTerms.Default, MockSettings, constructorArgs);
        }

        public T DxAs() 
        {
            return (T)_mock.Mock(typeof(T));
        }

        public class Returns<TMockType, TReturnType>
        {
            Action<TReturnType> Setter;
            Action Ensurer;
            MockBuilder<TMockType> Builder;

            public Returns(MockBuilder<TMockType> builder, Action<TReturnType> setter, Action ensurer)
            {
                Setter = setter;
                Ensurer = ensurer;
                Builder = builder;
            }

            public MockBuilder<TMockType> DxReturns(TReturnType value)
            {
                Setter(value);
                return Builder;
            }

            public MockBuilder<TMockType> DxEnsure()
            {
                if (Ensurer == null)
                    throw new InvalidOperationException("You cannot ensure this mocked item");

                Ensurer();
                return Builder;
            }
        }

        public IEnumerable<string> ShouldHaveBeenCalled
        {
            get { return _mock.ShouldHaveBeenCalled; }
        }
    }
}
