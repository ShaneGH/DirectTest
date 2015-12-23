using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Builders;
using Dynamox.Compile;
using Dynamox.Mocks;
using NUnit.Framework;

namespace Dynamox.Tests.Mocks
{
    [TestFixture]
    public class ObjectBaseValidatorTests
    {
        public class C0 { }
        public class C1 : C0 { }
        public class C2 : C1 { }

        public class AllGood<T>
            where T : C1
        {
            public virtual C1 Prop1 { get; set; }
            protected int Prop2 { private get; set; }

            internal int Prop3 { get; set; }
            public int Prop4 { set { } }
            public int Prop5 { get; private set; }
            private int Prop6 { get; set; }

            public int Field1;
            protected int Field2;
            internal int Field3;

            public virtual void Method1() { }
            public virtual void Method2(int val) { }
            public virtual void Method3(T val) { }
            public virtual void Method4<U>(U val)
                where U : C1
            { }

            public virtual C1 this[int key1, C1 key2] { get { return null; } set { } }
            public C1 this[C1 key2, int key1] { get { return null; } set { } }
        }

        [Test]
        public void AllGoodTests()
        {
            // arrange
            dynamic input = new MockBuilder();
            input.Prop1 = new C2();
            input.Prop2 = 1;
            input.Field1 = 1;
            input.Field2 = 1;
            input.Method1();
            input.Method2(4);
            input.Method3(new C2());
            input.Method4<C2>(new C2());
            input[1, new C2()] = new C2();
            input[new C2(), 1] = new C2();

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.IsEmpty(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values, input.IndexedValues)));
        }

        [Test]
        public void AllGoodTests_extended()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input.Prop1 = null;
            input[1, null] = new C2();
            input[new C2(), 1] = null;

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.IsEmpty(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values, input.IndexedValues)));
        }

        [Test]
        public void InvalidIndex_InvalidKeyType1()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input[new C0(), 1] = new C2();

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.AreEqual(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values, input.IndexedValues)).Count(), 1);
        }

        [Test]
        public void InvalidIndex_InvalidKeyType2()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input[new C2(), null] = new C2();

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.AreEqual(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values, input.IndexedValues)).Count(), 1);
        }

        [Test]
        public void InvalidIndex_InvalidValueType()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input[new C2(), 1] = new C0();

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.AreEqual(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values, input.IndexedValues)).Count(), 1);
        }

        [Test]
        public void InvalidType1()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input.Prop2 = null;

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.AreEqual(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values)).Count(), 1);
        }

        [Test]
        public void InvalidType2()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input.Prop1 = new C0();

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.AreEqual(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values)).Count(), 1);
        }

        [Test]
        public void InternalProp()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input.Prop3 = 1;

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.AreEqual(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values)).Count(), 1);
        }

        [Test]
        public void NoGetter()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input.Prop4 = 1;

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.AreEqual(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values)).Count(), 1);
        }

        [Test]
        public void NoSetter()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input.Prop5 = 1;

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.AreEqual(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values)).Count(), 1);
        }

        [Test]
        public void Private()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input.Prop6 = 1;

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.AreEqual(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values)).Count(), 1);
        }

        [Test]
        public void InternalField()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input.Field3 = 1;

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.AreEqual(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values)).Count(), 1);
        }

        [Test]
        public void InvalidFieldOrProp()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input.Invalid = 1;

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.AreEqual(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values)).Count(), 1);
        }

        [Test]
        public void TooManyArgs()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input.Method1(1);

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.AreEqual(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values)).Count(), 1);
        }

        [Test]
        public void NotEnoughArgs()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input.Method2();

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.AreEqual(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values)).Count(), 1);
        }

        [Test]
        public void InvalidArgTypes()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input.Method2("");

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.AreEqual(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values)).Count(), 1);
        }

        [Test]
        public void InvalidMethod()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input.InvalidMethod();

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.AreEqual(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values)).Count(), 1);
        }

        [Test]
        public void InvalidMethodGeneric()
        {
            // arrange
            dynamic input = new MockBuilder(Dx.Settings);
            input.Method4<int>(3);

            var subject = new ObjectBaseValidator(TypeOverrideDescriptor.Create(typeof(AllGood<>)));

            // act
            // assert
            Assert.AreEqual(subject.ValidateAgainstType(new ObjectBase(Dx.Settings, input.Values)).Count(), 1);
        }
    }
}
