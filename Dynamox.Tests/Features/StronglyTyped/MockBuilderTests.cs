using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;
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
            var mock = Dx.Mock<TestClass>()
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
        public void HybridBuilder()
        {
            // Arrange
            // Act
            var mock = Dx.Mock<TestClass>((strong, weak) =>
            {
                strong.Mock(x => x.Property1).DxReturns("val1");
                weak.Method1(5).DxReturns("val2");
            });

            // Assert
            Assert.AreEqual(mock.Property1, "val1");
            Assert.AreEqual(mock.Method1(5), "val2");
        }

        [Test]
        public void Ensure_Method()
        {
            // Arrange
            // Act
            var mock = Dx.Mock<TestClass>()
                .Mock(x => x.Method1(22)).DxReturns("val1").DxEnsure()
                .DxAs();

            // Act
            // Assert
            Assert.Throws<MockedMethodNotCalledException>(() => Dx.Ensure(mock));
            mock.Method1(11);
            Assert.Throws<MockedMethodNotCalledException>(() => Dx.Ensure(mock));
            mock.Method1(22);
            Dx.Ensure(mock);
        }

        [Test]
        public void Ensure_Property()
        {
            // Arrange
            // Act
            var mock = Dx.Mock<TestClass>()
                .Mock(x => x.Property1).DxReturns("val1").DxEnsure()
                .DxAs();

            // Act
            // Assert
            Assert.Throws<MockedMethodNotCalledException>(() => Dx.Ensure(mock));
            var p = mock.Property1;
            Dx.Ensure(mock);
        }

        [Test]
        public void Ensure_IndexedProperty()
        {
            // Arrange
            // Act
            var mock = Dx.Mock<TestClass>()
                .Mock(x => x["Hello"]).DxReturns("val1").DxEnsure()
                .DxAs();

            // Act
            // Assert
            Assert.Throws<MockedMethodNotCalledException>(() => Dx.Ensure(mock));
            var p = mock["Hello"];
            Dx.Ensure(mock);
        }

        [Test]
        public void Weak_Root()
        {
            // Arrange
            // Act
            var mock = Dx.Mock<TestClass>()
                .Mock(x => x.Method1(3)).DxReturns("val1");
            mock.WeakMock.Method1(4).DxReturns("val2");

            // Act
            var instance = mock.DxAs();

            // Assert
            Assert.AreEqual(instance.Method1(3), "val1");
            Assert.AreEqual(instance.Method1(4), "val2");
        }

        [Test]
        public void Weak_Property()
        {
            // Arrange
            // Act
            var mock = Dx.Mock<TestClass>()
                .Mock(x => x.Property2.Method1(3)).DxReturns("val1");
            mock.Mock(a => a.Property2).Weak.Method1(4).DxReturns("val2");

            // Act
            var instance = mock.DxAs();

            // Assert
            Assert.AreEqual(instance.Property2.Method1(3), "val1");
            Assert.AreEqual(instance.Property2.Method1(4), "val2");
        }

        [Test]
        public void Weak_IndexedProperty()
        {
            // Arrange
            // Act
            var mock = Dx.Mock<TestClass>()
                .Mock(x => x[1].Method1(3)).DxReturns("val1");
            mock.Mock(a => a[1]).Weak.Method1(4).DxReturns("val2");

            // Act
            var instance = mock.DxAs();

            // Assert
            Assert.AreEqual(instance[1].Method1(3), "val1");
            Assert.AreEqual(instance[1].Method1(4), "val2");
        }

        [Test]
        public void Weak_MethodMethod1()
        {
            // Arrange
            // Act
            var mock = Dx.Mock<TestClass>();
            mock.Mock(a => a.Method2()).Weak.Method1(4).DxReturns("val2");

            // Act
            var instance = mock.DxAs();

            // Assert
            Assert.AreEqual(instance.Method2().Method1(4), "val2");
        }

        [Test]
        public void Weak_MethodMethod2()
        {
            // Arrange
            // Act
            var mock = Dx.Mock<TestClass>();
            mock.Mock(a => a.Method2().Method1(4)).Weak.DxReturns("val2");

            // Act
            var instance = mock.DxAs();

            // Assert
            Assert.AreEqual(instance.Method2().Method1(4), "val2");
        }
    }
}
