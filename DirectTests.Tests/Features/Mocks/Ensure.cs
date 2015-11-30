using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DirectTests.Tests.Features.Mocks
{
    [TestFixture]
    public class Ensure
    {
        public interface ICurrentTest
        {
            void DoSomething(string val);
            ICurrentTest GetAnother();
        }

        [Test]
        public void Ok()
        {
            Framework.Test("")
                .Arrange(bag => bag.subject.DoSomething("Hello").Ensure())
                .Act(bag => { bag.subject.As<ICurrentTest>().DoSomething("Hello"); })
                .Run();
        }

        [Test]
        public void NotOk()
        {
            var test = Framework.Test("")
                .Arrange(bag => bag.subject.DoSomething("Hello").Ensure())
                .Act(bag => { bag.subject.As<ICurrentTest>().DoSomething("Not hello"); });

            Assert.Throws<InvalidOperationException>(() => test.Run());
        }

        [Test]
        public void Ok_Deep()
        {
            Framework.Test("")
                .Arrange(bag => bag.subject.GetAnother().Ensure().DoSomething("Hello").Ensure())
                .Act(bag => { bag.subject.As<ICurrentTest>().GetAnother().DoSomething("Hello"); })
                .Run();
        }

        [Test]
        public void NotOk_Deep()
        {
            var test = Framework.Test("")
                .Arrange(bag => bag.subject.GetAnother().DoSomething("Hello").Ensure())
                .Act(bag => { bag.subject.As<ICurrentTest>().GetAnother().DoSomething("Not hello"); });

            Assert.Throws<InvalidOperationException>(() => test.Run());
        }
    }
}