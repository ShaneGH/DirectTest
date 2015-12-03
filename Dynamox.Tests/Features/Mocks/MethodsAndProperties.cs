using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Dynamox.Tests.Features.Mocks
{
    [TestFixture]
    public class MethodsAndProperties
    {
        public interface ICurrentTest
        {
            ICurrentTest DoSomething();
            ICurrentTest GetSomething { get; }
            int Result { get; }
            int DoResult();
        }

        [Test]
        public void M_P_M_P()
        {
            Framework.Test("")
                .Arrange(bag => { bag.subject.DoSomething().GetSomething.DoSomething().Result = 8; })
                .Act(bag => bag.subject.As<ICurrentTest>().DoSomething().GetSomething.DoSomething().Result)
                .Assert((bag, result) => Assert.AreEqual(8, result))
                .Run();
        }

        [Test]
        public void P_M_P_M()
        {
            Framework.Test("")
                .Arrange(bag => { bag.subject.GetSomething.DoSomething().GetSomething.DoResult().Returns(8); })
                .Act(bag => bag.subject.As<ICurrentTest>().GetSomething.DoSomething().GetSomething.DoResult())
                .Assert((bag, result) => Assert.AreEqual(8, result))
                .Run();
        }

        [Test]
        public void M_M_M()
        {
            Framework.Test("")
                .Arrange(bag => { bag.subject.DoSomething().DoSomething().DoResult().Returns(8); })
                .Act(bag => bag.subject.As<ICurrentTest>().DoSomething().DoSomething().DoResult())
                .Assert((bag, result) => Assert.AreEqual(8, result))
                .Run();
        }

        [Test]
        public void P_P_P()
        {
            Framework.Test("")
                .Arrange(bag => { bag.subject.GetSomething.GetSomething.Result = 8; })
                .Act(bag => bag.subject.As<ICurrentTest>().GetSomething.GetSomething.Result)
                .Assert((bag, result) => Assert.AreEqual(8, result))
                .Run();
        }
    }
}
