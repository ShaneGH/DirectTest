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
    public class MockBuilder<T> : IEnsure, IMockBuilder<T>
    {
        readonly Mocks.Info.MockBuilder _mock;
        readonly List<Expression<Func<T, bool>>> MockExpressions = new List<Expression<Func<T, bool>>>();
        readonly DxSettings MockSettings;

        // use unique names for all reserved terms
        static readonly ReservedTerms UnobtrusiveReservedTerms = new ReservedTerms
        {
            DxAs = Guid.NewGuid().ToString(),
            DxClear = Guid.NewGuid().ToString(),
            DxConstructor = Guid.NewGuid().ToString(),
            DxDo = Guid.NewGuid().ToString(),
            DxEnsure = Guid.NewGuid().ToString(),
            DxOut = Guid.NewGuid().ToString(),
            DxReturns = Guid.NewGuid().ToString()
        };

        public dynamic WeakMock
        {
            get { return _mock; }
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

        public IReturns<T, object> Mock(Expression<Action<T>> mockExpression)
        {
            if (mockExpression == null)
                throw new InvalidOperationException("Invalid mock expression");

            return _Mock<object>(mockExpression.Body, mockExpression.Parameters[0]);
        }

        public IReturns<T, TReturnType> Mock<TReturnType>(Expression<Func<T, TReturnType>> mockExpression)
        {
            if (mockExpression == null)
                throw new InvalidOperationException("Invalid mock expression");

            return _Mock<TReturnType>(mockExpression.Body, mockExpression.Parameters[0]);
        }

        static List<Expression> InvertExpression(Expression mockExpression, ParameterExpression rootObject)
        {
            var expression = new List<Expression>();
            while (mockExpression != rootObject)
            {
                if (mockExpression is MemberExpression)
                {
                    expression.Insert(0, mockExpression);
                    mockExpression = (mockExpression as MemberExpression).Expression;
                }
                else if (mockExpression is MethodCallExpression)
                {
                    expression.Insert(0, mockExpression);
                    mockExpression = (mockExpression as MethodCallExpression).Object;
                }
                else
                {
                    throw new InvalidOperationException("Invalid mock expression");
                }
            }

            return expression;
        }

        object GetValueOfExpression(Expression mockExpression, ParameterExpression rootObject)
        {
            var expression = new Queue<Expression>(InvertExpression(mockExpression, rootObject));

            object current = _mock, val;
            while (expression.Any())
            {
                mockExpression = expression.Dequeue();

                if (mockExpression is MemberExpression)
                {
                    if ((current is MockBuilder))
                    {
                        var name = (mockExpression as MemberExpression).Member.Name;
                        if (!(current as MockBuilder).TryGetMember(name, out val))
                        {
                            (current as MockBuilder).SetMember(name, val = CreateMockBuilder(reservedTerms: UnobtrusiveReservedTerms));
                        }

                        current = val;
                    }
                    else if ((mockExpression as MemberExpression).Member is PropertyInfo)
                    {
                        current = ((mockExpression as MemberExpression).Member as PropertyInfo).GetGetMethod().Invoke(current, new object[0]);
                    }
                    else if ((mockExpression as MemberExpression).Member is FieldInfo)
                    {
                        current = ((mockExpression as MemberExpression).Member as FieldInfo).GetValue(current);
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid mock expression");
                    }
                }
                else if (mockExpression is MethodCallExpression)
                {
                    var args = GetValuesFromExpressions((mockExpression as MethodCallExpression).Arguments);
                    if (current is MockBuilder)
                    {
                        var asProperty = IsPropertyGetterOrSetter((mockExpression as MethodCallExpression).Method);
                        if (asProperty != null)
                        {
                            var tmp = CreateMockBuilder(reservedTerms: UnobtrusiveReservedTerms);
                            (current as MockBuilder).SetIndex(args, tmp);
                            current = tmp;
                        }
                        else
                        {
                            var name = (mockExpression as MethodCallExpression).Method.Name;
                            current = (current as MockBuilder)
                                .MockMethod(name, (mockExpression as MethodCallExpression).Method.GetGenericArguments(), args)
                                .ReturnValue = CreateMockBuilder(reservedTerms: UnobtrusiveReservedTerms);
                        }
                    }
                    else
                    {
                        current = (mockExpression as MethodCallExpression).Method.Invoke(current, args.ToArray());
                    }
                }
                else
                {
                    throw new InvalidOperationException("Invalid mock expression");
                }
            }

            return current;
        }

        IReturns<T, TReturnType> _Mock<TReturnType>(Expression mockExpression, ParameterExpression rootObject)
        {
            if (mockExpression == null)
                throw new InvalidOperationException("Invalid mock expression");

            try
            {
                _mock.MockSettings.Set(UnobtrusiveReservedTerms);

                var context = new MockValueSetter<T, TReturnType>(mockExpression);
                return context.Returns(this, GetValueOfExpression(context.LastGetter, rootObject));
            }
            finally
            {
                _mock.MockSettings.Set(new ReservedTerms());
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

        public IEnumerable<string> ShouldHaveBeenCalled
        {
            get { return _mock.ShouldHaveBeenCalled; }
        }

        static readonly MethodInfo DxAnyT1 = TypeUtils.GetMethod(() => Dx.AnyT<string>());
        static readonly MethodInfo DxAnyT2 = TypeUtils.GetMethod(() => Dx.AnyT(default(Type)));
        internal static bool IsDxAny(MethodInfo method)
        {
            return (method.IsGenericMethod && method.GetGenericMethodDefinition() == DxAnyT1) ||
                (!method.IsGenericMethod && method == DxAnyT2);
        }

        static readonly PropertyInfo DxAny = TypeUtils.GetProperty<object>(() => Dx.Any);
        internal static bool IsDxAny(MemberInfo property)
        {
            return property == DxAny;
        }

        internal static PropertyInfo IsPropertyGetterOrSetter(MethodInfo method)
        {
            return method.DeclaringType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(p => p.GetAccessors(true).Contains(method));
        }

        internal static IEnumerable<object> GetValuesFromExpressions(IEnumerable<Expression> expressions)
        {
            expressions = expressions ?? Enumerable.Empty<Expression>();

            return expressions.Select(a =>
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
        }
    }
}
