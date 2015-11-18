
//        //public void TestScenario()
//        //{
//        //    //return new TestBuilder("Base test").Build(testCase => testCase
//        //    //    .Configure(new Configuration { MismatchedMock = MismatchedMock.Exception })
//        //    //    .Inputs.arg1.Is(new object())
//        //    //    .Inputs.arg2.Does(i2 => i2.Val1.Val2.Val3(testCase.Any(), 2, testCase.Input1))
//        //    //        .AssertInputs(inputs => inputs.Get<string>(0) )
//        //    //        .AndReturns())
//        //    //    .Inputs.arg3.Is(testCase.Input_arg3.PartialMock())
//        //    //        .AndReturns());

            
//        //    // testCase.ConstructorArgs(CArgs), testCase.Args, testCase.Args.arg1.Value, testCase.Any()

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
//        //}
//    }

//    public class ClassToTest 
//    {
//        public ClassToTest(object arg1, string arg2) { }

//        public int MethodToTest(object arg1, int arg2) { return 5; }
//    }

//    public interface ITestCase
//    {
//        dynamic Args { get; }
//        dynamic ConstructorArgs { get; }
//        dynamic CArgs { get; }
//    }

//    public class TestCase : ITestCase
//    {
//        public Configuration Configuration { get; set; }
//        public dynamic Args { get; private set; }
//        public dynamic ConstructorArgs { get; private set; }
//        public dynamic CArgs { get; private set; }

//        internal static readonly object AnyResult = new Object(); 
//        public object Any() 
//        {
//            return AnyResult;
//        }

//        public bool Assert<T1, T2>(Func<T1, T2, bool> assert)
//        {
//            return true;
//        }

//        public bool Action<T1>(Action<T1> assert)
//        {
//            return true;
//        }
//    }

//    public class Configuration { }

//    public class ArgKeywords
//    {
//        public string Do { get; set; }
//        public string Ensure { get; set; }
//        public string Returns { get; set; }

//        public ArgKeywords()
//        {
//            Returns = "Returns";
//            Ensure = "Ensure";
//            Do = "Do";
//        }
//    }
//}
