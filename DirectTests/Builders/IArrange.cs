using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DirectTests.Dynamic;

namespace DirectTests.Builders
{
    public interface IArrange : ITest
    {
        IAct Arrange(Action<dynamic> arrange);

        IFor Subject(ConstructorInfo constructor);

        //IFor<TSubject> Subject<TSubject>(Expression<Func<TSubject>> constructor);
    }

    public interface IBasedOn : IArrange
    {
        IArrange BasedOn(string basedOn);
    }

    public interface IFor : ITest
    {
        IParameterizedArrange<object> For(MethodInfo method);
    }

    public interface IFor<TSubject>
    {
        IParameterizedArrange<TReturnValue> For<TReturnValue>(Expression<Func<TSubject, TReturnValue>> act);
    }

    public class TestData : ITestData
    {
        public dynamic TestBag { get; private set; }
        public dynamic Args { get; private set; }
        public dynamic CArgs { get; private set; }

        public TestData(DynamicBag testBag)
        {
            TestBag = testBag;
            Args = new TestArranger();
            CArgs = new TestArranger();
        }
    }

    public interface ITestData
    {
        dynamic TestBag { get; }
        dynamic Args { get; }
        dynamic CArgs { get; }
    }

    public interface IParameterizedArrange<TReturnValue> : ITest
    {
        IAssert<TReturnValue> Arrange(Action<ITestData> arrange);
    }

    //public interface IParameterizedAssert : ITest
    //{
    //    ITest Assert(Action<ITestData> assert);
    //}

    //public interface IParameterizedAssert<TActReturnType> : IParameterizedAssert
    //{
    //    ITest Assert(Action<ITestData, TActReturnType> assert);
    //}
}