using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Compile;
using Dynamox.Mocks;
using Dynamox.Mocks.Info;
using NUnit.Framework;
using COMPILER = Dynamox.Compile.Compiler;

namespace Dynamox.Tests.Compile.Compiler
{
    [TestFixture]
    public class Compiler_Methods
    {

        public abstract class LostOfMethods<T>
        {
            public string M1(int arg1) { return "O-" + arg1.ToString(); }
            private string M2(int arg1) { return "O-" + arg1.ToString(); }
            protected string M3(int arg1) { return "O-" + arg1.ToString(); }
            internal string M4(int arg1) { return "O-" + arg1.ToString(); }
            protected internal string M5(int arg1) { return "O-" + arg1.ToString(); }

            public virtual string VM1(int arg1) { return "O-" + arg1.ToString(); }
            protected virtual string VM3(int arg1) { return "O-" + arg1.ToString(); }
            internal virtual string VM4(int arg1) { return "O-" + arg1.ToString(); }
            protected internal virtual string VM5(int arg1) { return "O-" + arg1.ToString(); }

            public abstract string AM1(int arg1);
            protected abstract string AM3(int arg1);
            protected internal abstract string AM5(int arg1);

            public abstract T GM1(int arg1);

            public abstract TOut GM2<TOut>(int arg1);
        }

        [Test]
        public void LotsOfMethods()
        {
            Func<int, IEnumerable<Type>, MethodGroup> mock2 = (val, generic) =>
            {
                dynamic builder = new MethodMockBuilder(null, generic, new object[] { val });
                builder.DxReturns("M-" + val);

                return new MethodGroup(builder);
            };

            Func<int, MethodGroup> mock1 = val => mock2(val, Enumerable.Empty<Type>());
            
            var values = new MockBuilder();
            ((dynamic)values).M1 = mock1(2);
            ((dynamic)values).M4 = mock1(2);
            ((dynamic)values).M5 = mock1(2);

            ((dynamic)values).VM1 = mock1(2);
            ((dynamic)values).VM4 = mock1(2);
            ((dynamic)values).VM5 = mock1(2);

            ((dynamic)values).AM1 = mock1(2);
            ((dynamic)values).AM5 = mock1(2);

            ((dynamic)values).GM1 = mock1(2);
            ((dynamic)values).GM2 = mock2(2, new[] { typeof(string) });

            var subject = (LostOfMethods<string>)
                COMPILER.Compile(typeof(LostOfMethods<string>)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(DxSettings.GlobalSettings, values) });

            Assert.AreEqual(subject.M1(1), "O-1");
            Assert.AreEqual(subject.M1(2), "O-2");
            Assert.AreEqual(subject.M4(1), "O-1");
            Assert.AreEqual(subject.M4(2), "O-2");
            Assert.AreEqual(subject.M5(1), "O-1");
            Assert.AreEqual(subject.M5(2), "O-2");

            Assert.AreEqual(subject.VM1(1), "O-1");
            Assert.AreEqual(subject.VM1(2), "M-2");
            Assert.AreEqual(subject.VM4(1), "O-1");
            Assert.AreEqual(subject.VM4(2), "O-2");
            Assert.AreEqual(subject.VM5(1), "O-1");
            Assert.AreEqual(subject.VM5(2), "M-2");

            Assert.AreEqual(subject.AM1(1), default(string));
            Assert.AreEqual(subject.AM1(2), "M-2");
            Assert.AreEqual(subject.AM5(1), default(string));
            Assert.AreEqual(subject.AM5(2), "M-2");

            Assert.AreEqual(subject.GM1(1), default(string));
            Assert.AreEqual(subject.GM1(2), "M-2");
            Assert.AreEqual(subject.GM2<string>(1), default(string));
            Assert.AreEqual(subject.GM2<string>(2), "M-2");
            Assert.AreEqual(subject.GM2<object>(1), default(object));
            Assert.AreEqual(subject.GM2<object>(2), default(object));
        }

        public abstract class InternalAstractMethod
        {
            abstract internal int Prop11();
        }

        [Test]
        public void InternalAbstractMethod()
        {
            Assert.Throws(typeof(CompilerException), () =>
                COMPILER.Compile(typeof(InternalAstractMethod)));
        }

        public interface ILostOfMethods<T>
        {
            string M1(int arg1);
            T GM1(int arg1);
            TOut GM2<TOut>(int arg1);
        }

        [Test]
        public void ILotsOfMethods()
        {
            Func<int, IEnumerable<Type>, MethodGroup> mock2 = (val, generic) =>
            {
                dynamic builder = new MethodMockBuilder(null, generic, new object[] { val });
                builder.DxReturns("M-" + val);

                return new MethodGroup(builder);
            };

            Func<int, MethodGroup> mock1 = val => mock2(val, Enumerable.Empty<Type>());


            var values = new MockBuilder();
            ((dynamic)values).M1 = mock1(2);
            ((dynamic)values).GM1 = mock1(2);
            ((dynamic)values).GM2 = mock2(2, new[] { typeof(string) });

            var subject = (ILostOfMethods<string>)
                COMPILER.Compile(typeof(ILostOfMethods<string>)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(DxSettings.GlobalSettings, values) });

            Assert.AreEqual(subject.M1(1), default(string));
            Assert.AreEqual(subject.M1(2), "M-2");
            Assert.AreEqual(subject.GM1(1), default(string));
            Assert.AreEqual(subject.GM1(2), "M-2");
            Assert.AreEqual(subject.GM2<string>(1), default(string));
            Assert.AreEqual(subject.GM2<string>(2), "M-2");
            Assert.AreEqual(subject.GM2<object>(1), default(object));
            Assert.AreEqual(subject.GM2<object>(2), default(object));
        }

        [Test]
        public void ILotsOfMethods_NonExplicitMethods()
        {
            // arrange
            var mock = Dx.Mock();
            mock.M1(44).DxReturns("Hi");

            // act
            var result = mock.DxAs<ILostOfMethods<object>>().M1(44);

            // assert 
            Assert.AreEqual("Hi", result);
        }
    }
}