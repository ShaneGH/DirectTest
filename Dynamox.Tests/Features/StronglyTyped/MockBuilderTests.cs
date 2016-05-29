using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.StronglyTyped;
using NUnit.Framework;

namespace Dynamox.Tests.Features.StronglyTyped
{
    [TestFixture]
    public class MockBuilderTests
    {
        public  class TestClass
        {
            public virtual string this[string key]
            {
                get { return ""; }
                set { }
            }

            public virtual TestClass this[int key]
            {
                get { return null; }
                set { }
            }

            public virtual string Property1 { get; set; }
            public virtual string Method1(int val1)
            {
                return "RRR";
            }

            public virtual TestClass Method2()
            {
                return null;
            }

            public virtual TestClass Property2 { get; set; }
        }

        [Test]
        public void SmokeTests()
        {
            // Arrange
            // Act
            var mock = Dx.Strong<TestClass>()
                .Mock(x => x.Property1).DxReturns("val1")
                .Mock(x => x.Method1(Dx.AnyT<int>())).DxReturns("val3")
                .Mock(x => x.Method1(5)).DxReturns("val2")
                .Mock(x => x.Property2.Property1).DxReturns("val4")
                .Mock(x => x.Property2.Method1(Dx.AnyT<int>())).DxReturns("val5")
                .Mock(x => x["val6"]).DxReturns("val7")
                .Mock(x => x[Dx.AnyT<int>()].Property1).DxReturns("val8")
                .Mock(x => x[4].Property1).DxReturns("val9")
                .Mock(x => x.Method2().Property1).DxReturns("val10")
                .DxAs();

            // Assert
            Assert.AreEqual(mock.Property1, "val1");
            Assert.AreEqual(mock.Method1(5), "val2");
            Assert.AreEqual(mock.Method1(2), "val3");
            Assert.AreEqual(mock.Property2.Property1, "val4");
            Assert.AreEqual(mock.Property2.Method1(33), "val5");
            Assert.AreEqual(mock["val6"], "val7");
            Assert.AreEqual(mock[99].Property1, "val8");
            Assert.AreEqual(mock[4].Property1, "val9");
            Assert.AreEqual(mock.Method2().Property1, "val10");
        }

        [Test]
        public void StrongBuilder()
        {
            // Arrange
            // Act
            var mock = Dx.Strong<TestClass>(a => a.Mock(x => x.Property1).DxReturns("val1"));

            // Assert
            Assert.AreEqual(mock.Property1, "val1");
        }

        [Test]
        public void HybridBuilder()
        {
            // Arrange
            // Act
            var mock = Dx.Hybrid<TestClass>((strong, weak) =>
            {
                strong.Mock(x => x.Property1).DxReturns("val1");
                weak.Method1(5).DxReturns("val2");
            });

            // Assert
            Assert.AreEqual(mock.Property1, "val1");
            Assert.AreEqual(mock.Method1(5), "val2");
        }
    }
}
