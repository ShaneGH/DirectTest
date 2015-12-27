using System;
using NUnit.Framework;

namespace Dynamox.Tests.Features.Mocks
{
    [TestFixture]
    public class ConstructorArgs
    {
        public struct C1
        {
            public readonly string Arg;
            public C1(string arg)
            {
                Arg = arg;
            }
        }

        public class C2
        {
            public readonly string Arg;
            public C2(string arg)
            {
                Arg = arg;
            }

            public virtual C2 Chained { get { return null; } }
            public virtual C2 GetChained() { return null; }
        }

        [Test]
        public void Chained_Prop()
        {
            // Arrange
            var mock = Dx.Mock(new[] { "nothing" });
            mock.Chained.Constructor(new[] { "tada" }).ToString();

            // Act
            // Assert
            Assert.AreEqual(mock.As<C2>().Chained.Arg, "tada");
        }

        [Test]
        public void Chained_Function()
        {
            // Arrange
            var mock = Dx.Mock(new[] { "nothing" });
            mock.GetChained().Constructor(new[] { "tada" }).ToString();

            // Act
            // Assert
            Assert.AreEqual(mock.As<C2>().GetChained().Arg, "tada");
        }

        [Test]
        public void NoArgs_Struct()
        {
            // Arrange
            var mock = Dx.Mock();

            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => mock.As<C1>());
        }

        [Test]
        public void Args_Struct()
        {
            // Arrange
            var mock = Dx.Mock(new[] { "Hi" });

            // Act
            // Assert
            Assert.AreEqual(mock.As<C1>().Arg, "Hi");
        }

        [Test]
        public void NoArgs()
        {
            // Arrange
            var mock = Dx.Mock();

            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => mock.As<C2>());
        }

        [Test]
        public void Args()
        {
            // Arrange
            var mock = Dx.Mock(new[] { "Hi" });

            // Act
            // Assert
            Assert.AreEqual(mock.As<C2>().Arg, "Hi");
        }
    }
}