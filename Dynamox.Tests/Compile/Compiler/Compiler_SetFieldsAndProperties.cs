using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Compile;
using Dynamox.Mocks;
using NUnit.Framework;

using COMPILER = Dynamox.Compile.Compiler;

namespace Dynamox.Tests.Compile.Compiler
{
    [TestFixture]
    public class Compiler_SetFieldsAndProperties
    {
        public class C1 { public int val; public override string ToString() { return val.ToString(); } }
        public class C2 : C1 { public int val; }

        public class SetFields
        {
            protected string F1;
            protected int F2;
            internal string F3;

            public string GetF1() { return F1; }
            public int GetF2() { return F2; }
        }

        [Test]
        public void SetFieldsTests()
        {
            dynamic mocks = new MockBuilder();
            mocks.F1 = "Hi";
            mocks.F2 = "Hi";
            mocks.F3 = "Hi";

            var subject = (SetFields)
                COMPILER.Compile(typeof(SetFields)).GetConstructors()[0]
                    .Invoke(new object[] { new ObjectBase(new DxSettings { TestForInvalidMocks = false }, mocks.Values) });

            Assert.AreEqual(subject.GetF1(), "Hi");
            Assert.AreEqual(subject.GetF2(), 0);
            Assert.AreEqual(subject.F3, null);
        }

        public class SetProperties
        {
            protected string P1 { get; set; }
            protected int P2 { get; set; }
            internal string P3 { get; set; }
            public string P4 { private get; set; }
            public string P5 { set { } }
            public string P6 { get; private set; }

            public string GetP1() { return P1; }
            public int GetP2() { return P2; }
            public string GetP4() { return P4; }
        }

        [Test]
        public void SetPropertiesTests()
        {
            dynamic mocks = new MockBuilder();

            mocks.P1 = "Hi";
            mocks.P2 = "Hi";
            mocks.P3 = "Hi";
            mocks.P4 = "Hi";
            mocks.P5 = "Hi";
            mocks.P6 = "Hi";

            var subject = (SetProperties)
                COMPILER.Compile(typeof(SetProperties)).GetConstructors()[0]
                    .Invoke(new object[] { new ObjectBase(new DxSettings { TestForInvalidMocks = false }, mocks.Values) });

            Assert.AreEqual(subject.GetP1(), "Hi");
            Assert.AreEqual(subject.GetP2(), 0);
            Assert.AreEqual(subject.P3, null);
            Assert.AreEqual(subject.GetP4(), "Hi");
            Assert.AreEqual(subject.P6, null);
        }

        public class SetIndexes
        {
            readonly Dictionary<Tuple<C1, int>, C1> Vals1 = new Dictionary<Tuple<C1, int>, C1>();
            protected C1 this[C1 k1, int k2]
            {
                get { return Vals1[new Tuple<C1, int>(k1, k2)]; }
                set { Vals1.Add(new Tuple<C1, int>(k1, k2), value); }
            }

            readonly Dictionary<string, int> Vals2 = new Dictionary<string, int>();
            public int this[string k]
            {
                get { return Vals2[k]; }
                set { Vals2.Add(k, value); }
            }

            public C1 Get1(C1 key1, int key2)
            {
                return this[key1, key2];
            }
            //protected int F2;
            //internal string F3;
        }

        [Test]
        public void SetIndexesTests()
        {
            dynamic mocks = new MockBuilder();

            C1 key1 = new C1(), val1 = new C1();
            C2 key2 = new C2(), val2 = new C2();
            C1 key3 = null, val3 = null;
            mocks[key1, 33] = val1;
            mocks[key2, 55] = val2;
            mocks[key3, 66] = val3;

            mocks["hello"] = new { };
            mocks["goodbye"] = 989898;

            var copiled = COMPILER.Compile(typeof(SetIndexes));
            var subject = (SetIndexes)
                COMPILER.Compile(typeof(SetIndexes)).GetConstructors()[0]
                    .Invoke(new object[] { new ObjectBase(new DxSettings { TestForInvalidMocks = false }, mocks.IndexedValues) });

            Assert.AreEqual(subject.Get1(key1, 33), val1);
            Assert.AreEqual(subject.Get1(key2, 55), val2);
            Assert.AreEqual(subject.Get1(key3, 66), val3);

            Assert.AreEqual(subject["goodbye"], 989898);
        }
    }
}