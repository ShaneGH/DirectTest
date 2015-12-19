using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Dynamox.Tests.Features.Mocks
{
    [TestFixture]
    public class Returns
    {
        public interface ICurrentTest
        {
            string DoSomething(int val);
        }

        [Test]
        public void Returns_CorrectInput()
        {
            Dx.Test("")
                .Arrange(bag => bag.subject.DoSomething(33).Returns("hello"))
                .Act(bag => bag.subject.As<ICurrentTest>().DoSomething(33))
                .Assert((bag, val) => Assert.AreEqual(val, "hello"))
                .Run();
        }

        [Test]
        public void Returns_Null()
        {
            Dx.Test("")
                .Arrange(bag => bag.subject.DoSomething(33).Returns(null))
                .Act(bag => bag.subject.As<ICurrentTest>().DoSomething(33))
                .Assert((bag, val) => Assert.IsNull(val))
                .Run();
        }

        [Test]
        public void Returns_IncorrectInput()
        {
            Dx.Test("")
                .Arrange(bag => bag.subject.DoSomething(33).Returns("hello"))
                .Act(bag => bag.subject.As<ICurrentTest>().DoSomething(44))
                .Assert((bag, val) => Assert.AreEqual(val, null))
                .Run();
        }

        [Test]
        public void Returns_IncorrectReturnType()
        {
            var test = Dx.Test("")
                .Arrange(bag => bag.subject.DoSomething(33).Returns(new object()))
                .Act(bag => bag.subject.As<ICurrentTest>().DoSomething(33))
                .Assert((bag, val) => Assert.Fail());

            Assert.Throws<InvalidOperationException>(() => test.Run());
        }
    }
}
