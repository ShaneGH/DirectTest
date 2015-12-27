using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Dynamox.Tests.Features.Mocks
{
    [TestFixture]
    public class OutParamaters
    {
        public interface I1
        {
            string DoSomething(int val1, object ref1, ref int val2, ref object ref2, out int val3, out object ref3);
        }

        [Test]
        public void OutParamsVanilla()
        {
            // Arrange
            var mock = Dx.Mock();
            mock.DoSomething(3, Dx.Any, Dx.Any, Dx.Any, Dx.Any, Dx.Any)
                .Returns("44");

            int x;
            object y;
            int a = 9;
            object b = new object();

            // Act
            I1 obj = mock.As<I1>();
            var result = obj.DoSomething(3, new object(), ref a, ref b, out x, out y);

            // Assert
            Assert.AreEqual(result, "44");
        }

        [Test]
        public void OutParams_InvalidOut_Int()
        {
            // Arrange
            var mock = Dx.Mock();
            mock.DoSomething(3, Dx.Any, Dx.Any, Dx.Any, Dx.Any, Dx.Any)
                .Returns("44")
                .Out(2, new object());

            int x;
            object y;
            int a = 9;
            object b = new object();

            // Act
            I1 obj = mock.As<I1>();
            var result = obj.DoSomething(3, new object(), ref a, ref b, out x, out y);

            // Assert
            Assert.AreEqual(result, default(string));
        }

        [Test]
        [Ignore("TODO: not implemented")]
        public void OutParams_TestOuts_String()
        {
            // Arrange
            var out1 = new object();
            var out2 = new object();
            var mock = Dx.Mock();
            mock.DoSomething(3, Dx.Any, Dx.Any, Dx.Any, Dx.Any, Dx.Any)
                .Returns("44")
                .Out("val2", 77)
                .Out("ref2", out1)
                .Out("val3", 88)
                .Out("ref3", out2);

            int x;
            object y;
            int a = 9;
            object b = new object();

            // Act
            I1 obj = mock.As<I1>();
            var result = obj.DoSomething(3, new object(), ref a, ref b, out x, out y);

            // Assert
            Assert.AreEqual(result, "44");
            Assert.AreEqual(77, a);
            Assert.AreEqual(out1, b);
            Assert.AreEqual(88, x);
            Assert.AreEqual(out2, y);
        }

        [Test]
        public void OutParams_TestOuts_Int()
        {
            // Arrange
            var out1 = new object();
            var out2 = new object();
            var mock = Dx.Mock();
            mock.DoSomething(3, Dx.Any, Dx.Any, Dx.Any, Dx.Any, Dx.Any)
                .Returns("44")
                .Out(2, 77)
                .Out(3, out1)
                .Out(4, 88)
                .Out(5, out2);

            int x;
            object y;
            int a = 9;
            object b = new object();

            // Act
            I1 obj = mock.As<I1>();
            var result = obj.DoSomething(3, new object(), ref a, ref b, out x, out y);

            // Assert
            Assert.AreEqual(result, "44");
            Assert.AreEqual(77, a);
            Assert.AreEqual(out1, b);
            Assert.AreEqual(88, x);
            Assert.AreEqual(out2, y);
        }

        [Test]
        public void OutParams_RefNotSet()
        {
            // Arrange
            var mock = Dx.Mock();
            mock.DoSomething(3, Dx.Any, Dx.Any, Dx.Any, Dx.Any, Dx.Any)
                .Returns("44");

            int x;
            object y;
            int a = 9;
            object b = new object();
            object b1 = b;

            // Act
            I1 obj = mock.As<I1>();
            var result = obj.DoSomething(3, new object(), ref a, ref b, out x, out y);

            // Assert
            Assert.AreEqual(result, "44");
            Assert.AreEqual(9, a);
            Assert.AreEqual(b1, b);
        }
    }
}
