using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Compile;
using Dynamox.Mocks;

namespace Dynamox
{
    public class Program
    {
        public interface ICurrentTest
        {
            ICurrentTest DoSomething();
            ICurrentTest GetSomething { get; }
            int Result { get; }
        }

        public static void Main(string[] args)
        {
            Framework.Test("")
                .Arrange(bag => { bag.subject.DoSomething().GetSomething.DoSomething().Result = 8; })
                .Act(bag => bag.subject.As<ICurrentTest>().DoSomething().GetSomething.DoSomething().Result)
                .Assert((bag, result) => { })
                .Run();
        }
    }
}




//        //    new TestBuilderBase<ClassToTest, int>("Base test")
//        //        .Constructor(() => new ClassToTest(null, ""))
//        //        .Method(x => x.MethodToTest(null, 4))
//        //        .Build(testCase =>
//        //        {
//        //            testCase.Configuration = new Configuration();

//        //            testCase.Args.arg1 = new object();
//        //            testCase.CArgs.arg2.Val1.Val2.Val3(testCase.Any(), 2, testCase.Args.arg1)
//        //                .Ensure()
//        //                .Returns();

//        //            testCase.CArgs.arg3(new ArgKeywords { Ensure = "Banana" }).Val1.Val2.Val3(44)
//        //                .Do(testCase.Action<int>(a => { }))
//        //                .Banana();

//        //            testCase.CArgs.arg4.Val3(testCase.Assert<object, string>((a, b) => true));

//        //            testCase.Args.arg5 = new object();
//        //            testCase.Args.arg5.DoSomething()
//        //                .Returns(4);
//        //        })
//        //        .Assert((a, b) =>
//        //        {
//        //        })
//        //        .Assert((a, b, c) =>
//        //        {
//        //        });