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
    public class Ensure
    {
        public interface ICurrentTest
        {
            void DoSomething(string val);
            ICurrentTest GetAnother();
            ICurrentTest Another { get; }
        }

        [Test]
        public void Method_Ok()
        {
            Dx.Test("")
                .Arrange(bag => bag.subject.DoSomething("Hello").Ensure())
                .Act(bag => { bag.subject.As<ICurrentTest>().DoSomething("Hello"); })
                .Run();
        }

        [Test]
        public void Method_NotOk()
        {
            var test = Dx.Test("")
                .Arrange(bag => bag.subject.DoSomething("Hello").Ensure())
                .Act(bag => { bag.subject.As<ICurrentTest>().DoSomething("Not hello"); });

            Assert.Throws<InvalidOperationException>(() => test.Run());
        }

        [Test]
        public void Method_Ok_Deep()
        {
            Dx.Test("")
                .Arrange(bag => bag.subject.GetAnother().Ensure().DoSomething("Hello").Ensure())
                .Act(bag => { bag.subject.As<ICurrentTest>().GetAnother().DoSomething("Hello"); })
                .Run();
        }

        [Test]
        public void Method_NotOk_Deep()
        {
            var test = Dx.Test("")
                .Arrange(bag => bag.subject.GetAnother().DoSomething("Hello").Ensure())
                .Act(bag => { bag.subject.As<ICurrentTest>().GetAnother().DoSomething("Not hello"); });

            Assert.Throws<InvalidOperationException>(() => test.Run());
        }

        [Test]
        public void Method_Ok_AfterProperty()
        {
            Dx.Test("")
                .Arrange(bag => bag.subject.Another.DoSomething("Hello").Ensure())
                .Act(bag => { bag.subject.As<ICurrentTest>().Another.DoSomething("Hello"); })
                .Run();
        }

        [Test]
        public void Method_NotOk_AfterProperty()
        {
            var test = Dx.Test("")
                .Arrange(bag => bag.subject.Another.DoSomething("Hello").Ensure())
                .Act(bag => { bag.subject.As<ICurrentTest>().Another.DoSomething("Not hello"); });

            Assert.Throws<InvalidOperationException>(() => test.Run());
        }

        [Test]
        public void DxDotEnsure()
        {
            var mock1 = Dx.Mock();
            var mock2 = Dx.Mock();
            mock1.DoSomething("hello1").Ensure();
            mock2.DoSomething("hello2").Ensure();
            Assert.Throws<InvalidOperationException>(() => Dx.Ensure(mock1, mock2));

            mock1.As<ICurrentTest>().DoSomething("hello");
            mock2.As<ICurrentTest>().DoSomething("hello");
            Assert.Throws<InvalidOperationException>(() => Dx.Ensure(mock1, mock2));

            mock1.As<ICurrentTest>().DoSomething("hello1");
            mock2.As<ICurrentTest>().DoSomething("hello2");
            Dx.Ensure(mock1, mock2);
        }
    }
}