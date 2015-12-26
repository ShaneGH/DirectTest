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
    public class Do
    {
        public interface ICurrentTest
        {
            void DoSomething(string val);
        }

        [Test]
        public void CorrectInput()
        {
            Dx.Test("")
                .Arrange(bag => bag.subject.DoSomething("Hello").Do(Dx.Callback<string>(a =>
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
            Dx.Test("")
                .Arrange(bag => bag.subject.DoSomething(null).Do(Dx.Callback<string>(a =>
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
            Dx.Test("")
                .Arrange(bag => bag.subject.DoSomething("Hello").Do(Dx.Callback(() => bag.ok = true)))
                .Act(bag => { bag.subject.As<ICurrentTest>().DoSomething("Hello"); })
                .Assert(bag => Assert.IsTrue(bag.ok))
                .Run();
        }

        [Test]
        public void ParentTypeArg()
        {
            Dx.Test("")
                .Arrange(bag => bag.subject.DoSomething("Hello").Do(Dx.Callback<object>(a =>
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
            var test = Dx.Test("")
                .Arrange(bag => bag.subject.DoSomething("Hello").Do(Dx.Callback<int>(a => { })))
                .Act(bag => { bag.subject.As<ICurrentTest>().DoSomething("Hello"); });

            Assert.Throws<InvalidOperationException>(() => test.Run());
        }
    }
}