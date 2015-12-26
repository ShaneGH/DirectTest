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
        public void MethodPrecedence()
        {
            Dx.Test("")
                .Arrange(bag =>
                {
                    bag.subject.DoResult().Returns(33);
                    bag.subject.DoResult().Returns(44);
                })
                .Act(bag => (int)bag.subject.As<ICurrentTest>().DoResult())
                .Assert((bag, val) =>
                {
                    Assert.AreEqual(val, 44);
                })
                .Run();
        }

        [Test]
        public void M_P_M_P()
        {
            Dx.Test("")
                .Arrange(bag => { bag.subject.DoSomething().GetSomething.DoSomething().Result = 8; })
                .Act(bag => bag.subject.As<ICurrentTest>().DoSomething().GetSomething.DoSomething().Result)
                .Assert((bag, result) => Assert.AreEqual(8, result))
                .Run();
        }

        [Test]
        public void P_M_P_M()
        {
            Dx.Test("")
                .Arrange(bag => { bag.subject.GetSomething.DoSomething().GetSomething.DoResult().Returns(8); })
                .Act(bag => bag.subject.As<ICurrentTest>().GetSomething.DoSomething().GetSomething.DoResult())
                .Assert((bag, result) => Assert.AreEqual(8, result))
                .Run();
        }

        [Test]
        public void M_M_M()
        {
            Dx.Test("")
                .Arrange(bag => { bag.subject.DoSomething().DoSomething().DoResult().Returns(8); })
                .Act(bag => bag.subject.As<ICurrentTest>().DoSomething().DoSomething().DoResult())
                .Assert((bag, result) => Assert.AreEqual(8, result))
                .Run();
        }

        [Test]
        public void P_P_P()
        {
            Dx.Test("")
                .Arrange(bag => { bag.subject.GetSomething.GetSomething.Result = 8; })
                .Act(bag => bag.subject.As<ICurrentTest>().GetSomething.GetSomething.Result)
                .Assert((bag, result) => Assert.AreEqual(8, result))
                .Run();
        }

        [Test]
        public void M_M_Again()
        {
            Dx.Test("")
                .Arrange(bag => 
                {
                    bag.temp.DoResult().Returns(8);
                    bag.subject.DoSomething().Returns(bag.temp); 
                })
                .Act(bag => bag.subject.As<ICurrentTest>().DoSomething().DoResult())
                .Assert((bag, result) => Assert.AreEqual(8, result))
                .Run();
        }
    }
}
