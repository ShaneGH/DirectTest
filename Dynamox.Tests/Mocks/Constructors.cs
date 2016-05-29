using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Compile;
using Dynamox.Mocks;
using Dynamox.Mocks.Info;
using NUnit.Framework;

namespace Dynamox.Tests.Mocks
{
    [TestFixture]
    public class ConstructorsTests
    {
        public class C1
        {
            public string Prop;

            public C1(string prop)
            {
                Prop = prop;
            }

            public C1()
                : this("Hi")
            { }

            public C1(int nothing)
            { }
        }

        public sealed class C2
        {
            public string Prop;

            public C2(string prop)
            {
                Prop = prop;
            }

            public C2()
                : this("Hi")
            { }

            public C2(int nothing)
            { }
        }

        [Test]
        public void Non_Sealed()
        {
            // Arrange
            var subject = new Constructors(Compiler.Compile(typeof(C1)));
            var objBase1 = new ObjectBase(Dx.Settings);

            var values = new MockBuilder();
            ((dynamic)values).Prop = "bye";

            var objBase2 = new ObjectBase(Dx.Settings, values);

            // Act
            // Assert
            Assert.AreEqual("Hi", ((C1)subject.Construct(objBase1)).Prop);
            Assert.AreEqual("bla", ((C1)subject.Construct(objBase1, new[] { "bla" })).Prop);
            Assert.AreEqual(null, ((C1)subject.Construct(objBase1, new[] { 55 as object })).Prop);

            Assert.AreEqual("bye", ((C1)subject.Construct(objBase2)).Prop);
            Assert.AreEqual("bye", ((C1)subject.Construct(objBase2, new[] { "bla" })).Prop);
            Assert.AreEqual("bye", ((C1)subject.Construct(objBase2, new[] { 55 as object })).Prop);
        }

        [Test]
        public void Sealed()
        {
            // Arrange
            var subject = new Constructors(typeof(C2));
            var objBase1 = new ObjectBase(Dx.Settings);

            var values = new MockBuilder();
            ((dynamic)values).Prop = "bye";

            var objBase2 = new ObjectBase(Dx.Settings, values);

            // Act
            // Assert
            Assert.AreEqual("Hi", ((C2)subject.Construct(objBase1)).Prop);
            Assert.AreEqual("bla", ((C2)subject.Construct(objBase1, new[] { "bla" })).Prop);
            Assert.AreEqual(null, ((C2)subject.Construct(objBase1, new[] { 55 as object })).Prop);

            Assert.AreEqual("bye", ((C2)subject.Construct(objBase2)).Prop);
            Assert.AreEqual("bye", ((C2)subject.Construct(objBase2, new[] { "bla" })).Prop);
            Assert.AreEqual("bye", ((C2)subject.Construct(objBase2, new[] { 55 as object })).Prop);
        }

        //static ObjectBase Build(string prop, object value)
        //{
        //    var values = new MockBuilder();
        //    ((dynamic)values).M1 = mock1(2);

        //    return new ObjectBase(Dx.Settings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object> 
        //    {
        //        {prop, value}
        //    }));
        //}
    }
}
