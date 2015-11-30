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
    public class Do
    {
        public interface ICurrentTest
        {
            void DoSomething(string val);
        }

        [Test]
        public void CorrectInput()
        {
            Framework.Test("")
                .Arrange(bag => bag.subject.DoSomething("Hello").Do(Framework.Method<string>(a =>
                {
                    Assert.AreEqual("Hello", a);
                    bag.ok = true;
                })))
                .Act(bag => { bag.subject.As<ICurrentTest>().DoSomething("Hello"); })
                .Assert(bag => Assert.IsTrue(bag.ok))
                .Run();
        }

        [Test]
        public void NullInput()
        {
            Framework.Test("")
                .Arrange(bag => bag.subject.DoSomething(null).Do(Framework.Method<string>(a =>
                {
                    Assert.IsNull(a);
                    bag.ok = true;
                })))
                .Act(bag => { bag.subject.As<ICurrentTest>().DoSomething(null); })
                .Assert(bag => Assert.IsTrue(bag.ok))
                .Run();
        }

        [Test]
        public void NoInput()
        {
            Framework.Test("")
                .Arrange(bag => bag.subject.DoSomething("Hello").Do(Framework.Method(() => bag.ok = true)))
                .Act(bag => { bag.subject.As<ICurrentTest>().DoSomething("Hello"); })
                .Assert(bag => Assert.IsTrue(bag.ok))
                .Run();
        }

        [Test]
        public void ParentTypeArg()
        {
            Framework.Test("")
                .Arrange(bag => bag.subject.DoSomething("Hello").Do(Framework.Method<object>(a =>
                {
                    Assert.AreEqual("Hello", a);
                    bag.ok = true;
                })))
                .Act(bag => { bag.subject.As<ICurrentTest>().DoSomething("Hello"); })
                .Assert(bag => Assert.IsTrue(bag.ok))
                .Run();
        }

        [Test]
        public void InvalidTypeArg()
        {
            var test = Framework.Test("")
                .Arrange(bag => bag.subject.DoSomething("Hello").Do(Framework.Method<int>(a => { })))
                .Act(bag => { bag.subject.As<ICurrentTest>().DoSomething("Hello"); });

            Assert.Throws<InvalidOperationException>(() => test.Run());
        }
    }
}