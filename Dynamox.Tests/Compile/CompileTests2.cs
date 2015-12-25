﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Compile;
using Dynamox.Mocks;
using NUnit.Framework;

namespace Dynamox.Tests.Compile
{
    [TestFixture]
    public class CompileTests2
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
                Compiler.Compile(typeof(LostOfProperties<string>)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
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
                Compiler.Compile(typeof(InternalAstractProperty)));
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
                Compiler.Compile(typeof(ILostOfProperties<string>)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
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
                builder.Returns("M-" + val);

                return new MethodGroup(builder);
            };

            Func<int, MethodGroup> mock1 = val => mock2(val, Enumerable.Empty<Type>());

            var subject = (LostOfMethods<string>)
                Compiler.Compile(typeof(LostOfMethods<string>)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
                {
                    {"M1", mock1(2)},
                    {"M4", mock1(2)},
                    {"M5", mock1(2)},
                    
                    {"VM1", mock1(2)},
                    {"VM4", mock1(2)},
                    {"VM5", mock1(2)},
                    
                    {"AM1", mock1(2)},
                    {"AM5", mock1(2)},
                    
                    {"GM1", mock1(2)},
                    {"GM2", mock2(2, new[]{typeof(string)})}
                })) 
                });

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
            Assert.Throws(typeof(InvalidOperationException), () =>
                Compiler.Compile(typeof(InternalAstractMethod)));
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
                builder.Returns("M-" + val);

                return new MethodGroup(builder);
            };

            Func<int, MethodGroup> mock1 = val => mock2(val, Enumerable.Empty<Type>());

            var subject = (ILostOfMethods<string>)
                Compiler.Compile(typeof(ILostOfMethods<string>)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(DxSettings.GlobalSettings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
                {
                    {"M1", mock1(2)},                    
                    {"GM1", mock1(2)},
                    {"GM2", mock2(2, new[]{typeof(string)})}
                })) 
                });

            Assert.AreEqual(subject.M1(1), default(string));
            Assert.AreEqual(subject.M1(2), "M-2");
            Assert.AreEqual(subject.GM1(1), default(string));
            Assert.AreEqual(subject.GM1(2), "M-2");
            Assert.AreEqual(subject.GM2<string>(1), default(string));
            Assert.AreEqual(subject.GM2<string>(2), "M-2");
            Assert.AreEqual(subject.GM2<object>(1), default(object));
            Assert.AreEqual(subject.GM2<object>(2), default(object));
        }

        public interface IInterfaceAndClass
        {
            int Prop { get; set; }
            int Method();
        }

        public class InterfaceAndClass1
        {
            public virtual int Prop
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public virtual int Method()
            {
                throw new NotImplementedException();
            }
        }

        public class InterfaceAndClass2 : InterfaceAndClass1, IInterfaceAndClass
        {
        }

        [Test]
        public void InterfaceAndClassTests()
        {
            dynamic builder = new MockBuilder(Dx.Settings);
            builder.Prop = 77;
            builder.Method().Returns(88);

            var subject = (InterfaceAndClass2)Compiler.Compile(typeof(InterfaceAndClass2))
                    .GetConstructors()[0]
                        .Invoke(new object[] { new ObjectBase(Dx.Settings, builder.Values) });

            Assert.AreEqual(subject.Prop, 77);
            Assert.AreEqual(subject.Method(), 88);
        }

        public interface IExplicitInterface
        {
            int Prop { get; set; }
            int Method();
        }

        public class ExplicitInterface : IExplicitInterface
        {
            public virtual int Prop
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public virtual int Method()
            {
                throw new NotImplementedException();
            }

            int IExplicitInterface.Prop
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            int IExplicitInterface.Method()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void ExplicitInterfaceTests()
        {
            dynamic builder = new MockBuilder(Dx.Settings);
            builder.Prop = 77;
            builder.Method().Returns(88);

            var subject = (ExplicitInterface)Compiler.Compile(typeof(ExplicitInterface))
                    .GetConstructors()[0]
                        .Invoke(new object[] { new ObjectBase(Dx.Settings, builder.Values) });

            Assert.AreEqual(subject.Prop, 77);
            Assert.AreEqual(subject.Method(), 88);
            Assert.AreEqual(((IExplicitInterface)subject).Prop, 77);
            Assert.AreEqual(((IExplicitInterface)subject).Method(), 88);
        }

        public abstract class Indexes
        {
            public abstract int this[string val] { get; set; }
            public virtual int this[bool val] { get { throw new NotImplementedException(); } set { } }
        }

        [Test]
        public void IndexesTests()
        {
            var subject = (Indexes)
                Compiler.Compile(typeof(Indexes)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(Dx.Settings,
                    new ReadOnlyDictionary<IEnumerable<object>, object>(
                        new Dictionary<IEnumerable<object>, object>
                        {
                            {new object[]{"hello"}, 22},
                            {new object[]{true}, 44}
                        })) });

            Assert.AreEqual(subject["hello"], 22);
            subject["hello"] = 33;
            Assert.AreEqual(subject["hello"], 33);
            Assert.AreEqual(subject[true], 44);
        }

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
                Compiler.Compile(typeof(SetFields)).GetConstructors()[0]
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
                Compiler.Compile(typeof(SetProperties)).GetConstructors()[0]
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

            var copiled = Compiler.Compile(typeof(SetIndexes));
            var subject = (SetIndexes)
                Compiler.Compile(typeof(SetIndexes)).GetConstructors()[0]
                    .Invoke(new object[] { new ObjectBase(new DxSettings { TestForInvalidMocks = false }, mocks.IndexedValues) });

            Assert.AreEqual(subject.Get1(key1, 33), val1);
            Assert.AreEqual(subject.Get1(key2, 55), val2);
            Assert.AreEqual(subject.Get1(key3, 66), val3);

            Assert.AreEqual(subject["goodbye"], 989898);
        }
    }
}