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
        public abstract class LostOfProperties<T>
        {
            int _Prop0 = 1;
            public virtual int Prop0 { get { return _Prop0; } set { _Prop0 = value; } }

            int _Prop1 = 11;
            public virtual int Prop1 { get { return _Prop1; } set { _Prop1 = value; } }
            internal int Prop2 { get; set; }
            protected abstract int Prop3 { get; set; }
            private int Prop4 { get; set; }
            protected virtual internal int Prop5 { get; set; }
            public virtual int Prop6 { get; private set; }
            public virtual int Prop7 { get; protected set; }
            public virtual int Prop8 { get; internal set; }
            public abstract int Prop9 { get; }
            public abstract int Prop10 { set; }
        }

        public static void Main(string[] args)
        {
            var subject = (LostOfProperties<string>)
                Compiler.Compile(typeof(LostOfProperties<string>)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
                {
                    {"Prop1", 22},
                    {"Prop6", 33},
                    {"Prop7", 44},
                    {"Prop8", 55},
                    {"Prop9", 66}
                })) });

            //Assert.AreEqual(subject.Prop0, 1);
            //Assert.AreEqual(subject.Prop1, 22);
            //Assert.AreEqual(subject.Prop6, 33);
            //Assert.AreEqual(subject.Prop7, 44);
            //Assert.AreEqual(subject.Prop8, 55);
            //Assert.AreEqual(subject.Prop9, 66);

            //subject.Prop1 = 77;
            //Assert.AreEqual(subject.Prop1, 77);
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