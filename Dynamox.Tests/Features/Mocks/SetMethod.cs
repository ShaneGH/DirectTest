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
    }
}
