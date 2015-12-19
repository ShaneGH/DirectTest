using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Dynamic;
using Dynamox.Mocks;

namespace Dynamox.Builders
{
    public class SimpleTestBuilder : ITest, IFor
    {
        ConstructorInfo Constructor { get; set; }
        readonly TestBuilder Underlying;

        public SimpleTestBuilder(TestBuilder underlying)
        {
            Underlying = underlying;
        }

        public IFor Subject(ConstructorInfo constructor)
        {
            Constructor = constructor;
            return this;
        }

        public IFor<TSubject> Subject<TSubject>(Expression<Func<TSubject>> constructor)
        {
            var body = constructor.Body;
            while (body != null)
            {
                if (body.NodeType == ExpressionType.Convert)
                    body = (body as UnaryExpression).Operand;
                else
                    break;
            }

            if (!(body is NewExpression))
                throw new InvalidOperationException("Must be constructor"); // TODE

            Subject((body as NewExpression).Constructor);

            return new TypedSimpleTestBuilder<TSubject>(this);
        }

        private class TypedSimpleTestBuilder<TSubject> : IFor<TSubject>
        {
            readonly SimpleTestBuilder BasedOn;

            public TypedSimpleTestBuilder(SimpleTestBuilder basedOn)
            {
                BasedOn = basedOn;
            }

            static MethodInfo GetMethod(Expression fromExpression)
            {
                while (fromExpression != null)
                {
                    if (fromExpression.NodeType == ExpressionType.Convert)
                        fromExpression = (fromExpression as UnaryExpression).Operand;
                    else
                        break;
                }

                //TODO: extend to allow property calls
                if (!(fromExpression is MethodCallExpression))
                    throw new InvalidOperationException("Must be a method call"); // TODE

                return (fromExpression as MethodCallExpression).Method;
            }

            public IParameterizedArrange<TReturnValue> For<TReturnValue>(Expression<Func<TSubject, TReturnValue>> act)
            {
                return BasedOn.For<TReturnValue>(GetMethod(act.Body));
            }

            public IParameterizedArrange For(Expression<Action<TSubject>> act)
            {
                return BasedOn.For<object>(GetMethod(act.Body));
            }

            public TestBuilder Builder
            {
                get { return BasedOn.Builder; }
            }

            public void Run()
            {
                Framework.Run(this);
            }
        }

        public IParameterizedArrange<object> For(MethodInfo method)
        {
            return For<object>(method);
        }

        /// <summary>
        /// Do not make public, takes some liberties with casting which will not be a problem if private
        /// </summary>
        /// <typeparam name="TReturnVal"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        private IParameterizedArrange<TReturnVal> For<TReturnVal>(MethodInfo method)
        {
            var allMethods = Constructor.DeclaringType.AllClassesAndInterfaces().SelectMany(t => t.GetMethods());
            if (!allMethods.Contains(method))
                throw new InvalidOperationException("Invalid method");

            var action = Underlying.Act<TReturnVal>(testBag =>
            {
                //TODO: better way (Dynamox_Builders_IParameterizedArrange) 
                var args = testBag.Dynamox_Builders_IParameterizedArrange;

                var constructorArgs = GetArgs(Constructor.GetParameters(), (TestArranger)args.CArgs).ToArray();
                var methodArgs = GetArgs(method.GetParameters(), (TestArranger)args.Args).ToArray();

                //TODO: can I call explicitly implemented methods this way?
                return (TReturnVal)method.Invoke(Constructor.Invoke(constructorArgs), methodArgs);
            });

            return new Actor<TReturnVal>(this, action);
        }

        static IEnumerable<object> GetArgs(IEnumerable<ParameterInfo> parameters, DynamicBag data)
        {
            foreach (var param in parameters)
            {
                var value = data.Values.ContainsKey(param.Name) ?
                    data.Values[param.Name] :
                    (param.ParameterType.IsValueType ? Activator.CreateInstance(param.ParameterType) : null);

                if (value is MockBuilder)
                    value = (value as MockBuilder).Mock(param.ParameterType);

                if ((value == null && param.ParameterType.IsValueType) ||
                    (value != null && !param.ParameterType.IsAssignableFrom(value.GetType())))
                    throw new InvalidOperationException("Invalid type");    //TODE

                yield return value;
            }
        }

        private class Actor<TReturnValue> : IParameterizedArrange<TReturnValue>//, IParameterizedAssert<TReturnValue>
        {
            readonly SimpleTestBuilder Underlying;
            readonly IAssert<TReturnValue> Asserter;

            public TestBuilder Builder
            {
                get { return Underlying.Builder; }
            }

            public Actor(SimpleTestBuilder underlying, IAssert<TReturnValue> asserter)
            {
                Underlying = underlying;
                Asserter = asserter;
            }

            public IAssert<TReturnValue> Arrange(Action<ITestData> arrange)
            {
                Builder.Arrange(testBag =>
                {
                    var data = new TestData(testBag);
                    testBag.Dynamox_Builders_IParameterizedArrange = data;
                    arrange(data);
                });

                return Asserter;
            }

            IAssert IParameterizedArrange.Arrange(Action<ITestData> arrange)
            {
                return Arrange(arrange);
            }

            public void Run()
            {
                Framework.Run(this);
            }
        }

        public TestBuilder Builder
        {
            get { return Underlying; }
        }

        public void Run()
        {
            Framework.Run(this);
        }
    }
}