using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;
using NUnit.Framework;

namespace Dynamox.Tests.Features.Mocks
{
    [TestFixture]
    public class SetMethod
    {
        public interface ITest
        {
            int MethodWithReturnType(string input);

            void MethodWithNoReturnType(string input);

            void GenericMethodWithNoReturnType<T>(T input);

            T2 GenericMethodWithReturnType<T1, T2>(T1 input);
        }

        [Test]
        public void SetMethod_Ensure()
        {
            // Arrange
            var mock = Dx.Mock();
            mock.MethodWithReturnType = Dx.Method<string, int>(a =>
            {
                Assert.AreEqual("5", a);
                return 88;
            }).DxEnsure();

            ITest mocked = mock.DxAs<ITest>();

            Assert.Throws<MockedMethodNotCalledException>(() => Dx.Ensure(mocked));

            // Act
            mocked.MethodWithReturnType("5");

            // Assert
            Dx.Ensure(mocked);
        }

        [Test]
        public void SetMethod_WithReturnType()
        {
            // Arrange
            var mock = Dx.Mock();
            mock.MethodWithReturnType = Dx.Method<string, int>(a =>
            {
                Assert.AreEqual("5", a);
                return 88;
            });

            ITest mocked = mock.DxAs<ITest>();

            // Act
            var val = mocked.MethodWithReturnType("5");

            // Assert
            Assert.AreEqual(88, val);
        }

        [Test]
        public void SetVoidMethod_WithEnsure()
        {
            // Arrange
            var called = false;
            var mock = Dx.Mock();
            mock.MethodWithNoReturnType = Dx.VoidMethod<string>(a =>
            {
                called = true;
                Assert.AreEqual("5", a);
            }).DxEnsure();

            ITest mocked = mock.DxAs<ITest>();

            Assert.Throws<MockedMethodNotCalledException>(() => Dx.Ensure(mocked));

            // Act
            mocked.MethodWithNoReturnType("5");

            // Assert
            Dx.Ensure(mocked);
            Assert.True(called);
        }

        [Test]
        public void SetMethod_Generic_WithReturnType()
        {
            // Arrange
            var mock = Dx.Mock();
            mock.GenericMethodWithReturnType = Dx.Method<string, int>(a =>
            {
                Assert.AreEqual("5", a);
                return 88;
            }).AddGeneric<string>().And<int>();

            ITest mocked = mock.DxAs<ITest>();

            // Act
            var val = mocked.GenericMethodWithReturnType<string, int>("5");

            // Assert
            Assert.AreEqual(88, val);
        }

        [Test]
        public void SetVoidMethod_Generic_WithEnsure()
        {
            // Arrange
            var called = false;
            var mock = Dx.Mock();
            mock.GenericMethodWithNoReturnType = Dx.VoidMethod<string>(a =>
            {
                called = true;
                Assert.AreEqual("5", a);
            }).AddGeneric<string>().DxEnsure();

            ITest mocked = mock.DxAs<ITest>();

            Assert.Throws<MockedMethodNotCalledException>(() => Dx.Ensure(mocked));

            // Act
            mocked.GenericMethodWithNoReturnType("5");

            // Assert
            Dx.Ensure(mocked);
            Assert.True(called);
        }
    }
}
