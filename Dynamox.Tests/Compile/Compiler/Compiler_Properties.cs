using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;
using NUnit.Framework;
using COMPILER = Dynamox.Compile.Compiler;

namespace Dynamox.Tests.Compile.Compiler
{
    [TestFixture]
    public class Compiler_Properties
    {
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
            protected abstract internal int Prop11 { get; set; }

            public virtual T Prop12 { get; set; }

            public abstract int this[string val] { get; set; }
        }

        [Test]
        public void LotsOfProperties()
        {
            var subject = (LostOfProperties<string>)
                COMPILER.Compile(typeof(LostOfProperties<string>)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
                {
                    {"Prop1", 22},
                    {"Prop6", 33},
                    {"Prop7", 44},
                    {"Prop8", 55},
                    {"Prop9", 66},
                    {"Prop11", 88},
                    {"Prop12", "hello"}
                })) });

            Assert.AreEqual(subject.Prop0, 1);
            Assert.AreEqual(subject.Prop1, 22);
            Assert.AreEqual(subject.Prop6, 33);
            Assert.AreEqual(subject.Prop7, 44);
            Assert.AreEqual(subject.Prop8, 55);
            Assert.AreEqual(subject.Prop9, 66);
            Assert.AreEqual(subject.Prop11, 88);
            Assert.AreEqual(subject.Prop12, "hello");

            subject.Prop1 = 77;
            Assert.AreEqual(subject.Prop1, 77);

            subject.Prop11 = 99;
            Assert.AreEqual(subject.Prop11, 99);

            subject.Prop12 = "goodbye";
            Assert.AreEqual(subject.Prop12, "goodbye");
        }

        public abstract class InternalAstractProperty
        {
            abstract internal int Prop11 { get; set; }
        }

        [Test]
        public void InternalAbstractProperty()
        {
            Assert.Throws(typeof(InvalidOperationException), () =>
                COMPILER.Compile(typeof(InternalAstractProperty)));
        }
        public interface ILostOfProperties<T>
        {
            int Prop0 { get; set; }
            int Prop1 { set; }
            int Prop2 { get; }

            T Prop3 { get; set; }
        }

        [Test]
        public void ILotsOfProperties()
        {
            var subject = (ILostOfProperties<string>)
                COMPILER.Compile(typeof(ILostOfProperties<string>)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
                {
                    {"Prop0", 22},
                    {"Prop1", 33},
                    {"Prop2", 44}
                    ,
                    {"Prop3", "hello"}
                })) });

            Assert.AreEqual(subject.Prop0, 22);
            Assert.AreEqual(subject.Prop2, 44);
            Assert.AreEqual(subject.Prop3, "hello");

            subject.Prop1 = 66;
            subject.Prop0 = 77;
            Assert.AreEqual(subject.Prop0, 77);

            subject.Prop3 = "somethinfg";
            Assert.AreEqual(subject.Prop3, "somethinfg");
        }
    }
}
