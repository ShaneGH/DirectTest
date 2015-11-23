using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DirectTests.Dynamic;
using DirectTests.Mocks;

namespace DirectTests.Builders
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

        public IParameterizedArrange<object> For(MethodInfo method)
        {
            var allMethods = AllClassesAndInterfaces(Constructor.DeclaringType).SelectMany(t => t.GetMethods());
            if (!allMethods.Contains(method))
                throw new InvalidOperationException("Invalid method");

            var action = Underlying.Act<object>(testBag =>
            {
                //TODO: better way (DirectTests_Builders_IParameterizedArrange) 
                var args = testBag.DirectTests_Builders_IParameterizedArrange;

                var constructorArgs = GetArgs(Constructor.GetParameters(), (TestArranger)args.CArgs).ToArray();
                var methodArgs = GetArgs(method.GetParameters(), (TestArranger)args.Args).ToArray();

                //TODO: can I call explicitly implemented methods this way?
                return method.Invoke(Constructor.Invoke(constructorArgs), methodArgs);
            });

            return new Actor<object>(this, action);
        }

        static IEnumerable<Type> AllClassesAndInterfaces(Type type)
        {
            var output = new List<Type>();
            while (type != null)
            {
                output.Add(type);
                output.AddRange(type.GetInterfaces());
                type = type.BaseType;
            }

            return output.Where(o => o != null).Distinct();
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
                    throw new InvalidOperationException("Invalid type");    //TODO

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
                    testBag.DirectTests_Builders_IParameterizedArrange = data;
                    arrange(data);
                });

                return Asserter;
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