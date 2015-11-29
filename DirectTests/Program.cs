using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DirectTests.Compile;
using DirectTests.Mocks;

namespace DirectTests
{
    public class Program
    {

        //TODO: mock or rather set fields (comment not really related to this class)
        public abstract class LostOfMethods<T>
        {
            public abstract TOut GM2<TOut>(int arg1);
        }

        public static void Main(string[] args)
        {

            Func<int, IEnumerable<Type>, MethodGroup> mock2 = (val, generic) =>
            {
                dynamic builder = new MethodMockBuilder(null, generic, new object[] { val });
                builder.Return("M-" + val);

                return new MethodGroup(builder);
            };

            Func<int, MethodGroup> mock1 = val => mock2(val, Enumerable.Empty<Type>());

            var subject = (LostOfMethods<string>)
                Compiler.Compile(typeof(LostOfMethods<string>)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
                {
                    //{"M1", mock(2)},
                    //{"M4", mock(2)},
                    //{"M5", mock(2)},
                    
                    //{"VM1", mock(2)},
                    //{"VM4", mock(2)},
                    //{"VM5", mock(2)},
                    
                    //{"AM1", mock(2)},
                    //{"AM5", mock(2)},
                    
                    //{"GM1", mock(2)},
                    {"GM2", mock2(2, new[]{typeof(string)})},
                })) });

            subject.GM2<string>(2);
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