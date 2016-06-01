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
            T Generic<T>(out T val1, ref T val2);
        }

        [Test]
        public void OutParamsVanilla()
        {
            // Arrange
            var mock = Dx.Mock();
            mock.DoSomething(3, Dx.Any(), Dx.Any(), Dx.Any(), Dx.Any(), Dx.Any())
                .DxReturns("44");

            int x;
            object y;
            int a = 9;
            object b = new object();

            // Act
            I1 obj = mock.DxAs<I1>();
            var result = obj.DoSomething(3, new object(), ref a, ref b, out x, out y);

            // Assert
            Assert.AreEqual(result, "44");
        }

        [Test]
        public void OutParams_InvalidOut_Int()
        {
            // Arrange
            var mock = Dx.Mock();
            mock.DoSomething(3, Dx.Any(), Dx.Any(), Dx.Any(), Dx.Any(), Dx.Any())
                .DxReturns("44")
                .DxOut(2, new object());

            int x;
            object y;
            int a = 9;
            object b = new object();

            // Act
            I1 obj = mock.DxAs<I1>();
            var result = obj.DoSomething(3, new object(), ref a, ref b, out x, out y);

            // Assert
            Assert.AreEqual(result, default(string));
        }

        [Test]
        public void OutParams_TestOuts_String()
        {
            // Arrange
            var out1 = new object();
            var out2 = "rwerewrwrew";
            var mock = Dx.Mock();
            mock.DoSomething(3, Dx.Any(), Dx.Any(), Dx.Any(), Dx.Any(), Dx.Any())
                .DxReturns("44")
                .DxOut("val2", 77)
                .DxOut("ref2", out1)
                .DxOut("val3", 88)
                .DxOut("ref3", out2);

            int x;
            object y;
            int a = 9;
            object b = new object();

            // Act
            I1 obj = mock.DxAs<I1>();
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
            var out2 = "rwerewrwrew";
            var mock = Dx.Mock();
            mock.DoSomething(3, Dx.Any(), Dx.Any(), Dx.Any(), Dx.Any(), Dx.Any())
                .DxReturns("44")
                .DxOut(2, 77)
                .DxOut(3, out1)
                .DxOut(4, 88)
                .DxOut(5, out2);

            int x;
            object y;
            int a = 9;
            object b = new object();

            // Act
            I1 obj = mock.DxAs<I1>();
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
            mock.DoSomething(3, Dx.Any(), Dx.Any(), Dx.Any(), Dx.Any(), Dx.Any())
                .DxReturns("44");

            int x;
            object y;
            int a = 9;
            object b = new object();
            object b1 = b;

            // Act
            I1 obj = mock.DxAs<I1>();
            var result = obj.DoSomething(3, new object(), ref a, ref b, out x, out y);

            // Assert
            Assert.AreEqual(result, "44");
            Assert.AreEqual(9, a);
            Assert.AreEqual(b1, b);
        }

        [Test]
        public void OutParams_Generic_RefTypes()
        {
            // Arrange
            object x1, x2 = new object(), y1 = new object(), y2 = new object(), output = new object();
            var mock = Dx.Mock();
            mock.Generic<object>(Dx.Any(), Dx.Any())
                .DxReturns(output)
                .DxOut(0, x2)
                .DxOut(1, y2);

            int y = 9;

            // Act
            I1 obj = mock.DxAs<I1>();
            var result = obj.Generic<object>(out x1, ref y1);

            // Assert
            Assert.AreEqual(result, output);
            Assert.AreEqual(x1, x2);
            Assert.AreEqual(y1, y2);
        }

        [Test]
        public void OutParams_Generic_ValTypes()
        {
            // Arrange
            var mock = Dx.Mock();
            mock.Generic<int>(Dx.Any(), Dx.Any())
                .DxReturns(44)
                .DxOut(0, 55)
                .DxOut(1, 77);

            int x;
            int y = 9;

            // Act
            I1 obj = mock.DxAs<I1>();
            var result = obj.Generic<int>(out x, ref y);

            // Assert
            Assert.AreEqual(result, 44);
            Assert.AreEqual(55, x);
            Assert.AreEqual(77, y);
        }
    }
}
