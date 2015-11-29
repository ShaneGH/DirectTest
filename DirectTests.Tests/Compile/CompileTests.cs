﻿using System;
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
        }

        [Test]
        public void LotsOfProperties()
        {
            var subject = (LostOfProperties<string>)
                Compiler.Compile(typeof(LostOfProperties<string>)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
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

            //TODO assert
        }
        public interface Interfce<T>
        {
            int Prop0 { get; set; }
            int Prop1 { set; }  //TODO: callback on interface accessor
            int Prop2 { get; }

            T Prop3 { get; set; }
        }

        [Test]
        public void Interface()
        {
            var subject = (Interfce<string>)
                Compiler.Compile(typeof(Interfce<string>)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
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

        //TODO: mock or rather set fields (comment not really related to this class)
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
                Compiler.Compile(typeof(LostOfMethods<string>)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
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

            //TODO assert
        }

        //TODO: mock or rather set fields (comment not really related to this class)
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
                Compiler.Compile(typeof(ILostOfMethods<string>)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
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
    }
}