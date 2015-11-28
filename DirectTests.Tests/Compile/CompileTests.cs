using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectTests.Compile;
using DirectTests.Mocks;
using NUnit.Framework;

namespace DirectTests.Tests.Compile
{
    [TestFixture]
    public class CompileTests
    {
        //TODO: mock or rather set fields (comment not really related to this class)
        public abstract class LostOfProperties
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

        [Test]
        public void LotsOfProperties()
        {
            var subject = (LostOfProperties)
                Compiler.Compile(typeof(LostOfProperties)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
                {
                    {"Prop1", 22},
                    {"Prop6", 33},
                    {"Prop7", 44},
                    {"Prop8", 55},
                    {"Prop9", 66}
                })) });

            Assert.AreEqual(subject.Prop0, 1);
            Assert.AreEqual(subject.Prop1, 22);
            Assert.AreEqual(subject.Prop6, 33);
            Assert.AreEqual(subject.Prop7, 44);
            Assert.AreEqual(subject.Prop8, 55);
            Assert.AreEqual(subject.Prop9, 66);

            subject.Prop1 = 77;
            Assert.AreEqual(subject.Prop1, 77);
        }

        public abstract class InternalAstract
        {
            protected abstract internal int Prop11 { get; set; }
        }

        [Test]
        public void InternalAbstract()
        {
            Assert.Throws(typeof(InvalidOperationException), () =>
                Compiler.Compile(typeof(InternalAstract)));

            //TODO assert
        }
        public interface Interfce
        {
            int Prop0 { get; set; }
            int Prop1 { set; }  //TODO: callback on interface accessor
            int Prop2 { get; }
        }

        [Test]
        public void Interface()
        {
            var subject = (Interfce)
                Compiler.Compile(typeof(Interfce)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
                {
                    {"Prop0", 22},
                    {"Prop1", 33},
                    {"Prop2", 44}
                })) });

            Assert.AreEqual(subject.Prop0, 22);
            Assert.AreEqual(subject.Prop2, 44);

            subject.Prop1 = 66;
            subject.Prop0 = 77;
            Assert.AreEqual(subject.Prop0, 77);
        }
    }
}