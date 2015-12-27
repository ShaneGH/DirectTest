using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Dynamox.Tests.Features.Mocks
{
    [TestFixture]
    public class SettingsTests
    {
        public interface ICurrentTest 
        {
            int DoSomething(int val);
        }

        [Test]
        public void Returns()
        {
            Dx.Test("")
                .Arrange(bag =>
                {
                    bag.subject(new MockSettings { Returns = "baboon" }).DoSomething(22).baboon(33);
                    bag.subject(new { Returns = "baboon" }).DoSomething(44).baboon(55);
                })
                .Act(bag =>
                {
                    bag.v1 = bag.subject.As<ICurrentTest>().DoSomething(22);
                    bag.v2 = bag.subject.As<ICurrentTest>().DoSomething(44);
                })
                .Assert((bag) =>
                {
                    Assert.AreEqual(bag.v2, 55);
                    Assert.AreEqual(bag.v1, 33);
                })
                .Run();
        }

        [Test]
        public void Ensure()
        {
            var module = Dx.Module();
            module.Add("pass")
                .Arrange(bag =>
                {
                    bag.subject(new MockSettings { Ensure = "baboon" }).DoSomething(22).Returns(33).baboon();
                    bag.subject(new { Ensure = "baboon" }).DoSomething(44).Returns(55).baboon();
                })
                .Act(bag =>
                {
                    bag.v1 = bag.subject.As<ICurrentTest>().DoSomething(22);
                    bag.v2 = bag.subject.As<ICurrentTest>().DoSomething(44);
                })
                .Assert((bag) =>
                {
                    Assert.AreEqual(bag.v2, 55);
                    Assert.AreEqual(bag.v1, 33);
                });

            module.Add("As MockSettings fail")
                .BasedOn("pass")
                .Arrange(bag => { })
                .UseParentAct(false)
                .Act(bag =>
                {
                    bag.v2 = bag.subject.As<ICurrentTest>().DoSomething(44);
                })
                .SkipParentAssert();

            module.Add("As anonymous fail")
                .BasedOn("As MockSettings fail")
                .Arrange(bag => { })
                .UseParentAct(false)
                .Act(bag =>
                {
                    bag.v2 = bag.subject.As<ICurrentTest>().DoSomething(22);
                })
                .SkipParentAssert(false);

            Dx.Run(module, "pass");
            Assert.Throws<InvalidOperationException>(() => Dx.Run(module, "As MockSettings fail"));
            Assert.Throws<InvalidOperationException>(() => Dx.Run(module, "As anonymous fail"));
        }

        [Test]
        public void Clear1()
        {
            Dx.Test("")
                .Arrange(bag =>
                {
                    bag.subject.DoSomething(22).Returns(33);
                    bag.subject(new MockSettings { Clear = "gorilla" }).gorilla();
                    bag.subject.DoSomething(44).Returns(55);
                    bag.subject(new { Clear = "gorilla" }).gorilla();
                })
                .Act(bag =>
                {
                    bag.v1 = bag.subject.As<ICurrentTest>().DoSomething(22);
                    bag.v2 = bag.subject.As<ICurrentTest>().DoSomething(44);
                })
                .Assert((bag) =>
                {
                    Assert.AreEqual(bag.v1, 0);
                    Assert.AreEqual(bag.v2, 0);
                })
                .Run();
        }

        [Test]
        public void Do()
        {
            Dx.Test("")
                .Arrange(bag =>
                {
                    bag.v3 = false;
                    bag.subject(new MockSettings { Do = "baboon" }).DoSomething(22).baboon(Dx.Callback(() => bag.v1 = true)).Returns(33);
                    bag.subject(new { Do = "baboon" }).DoSomething(44).baboon(Dx.Callback(() => bag.v2 = true)).Returns(55);
                    bag.subject(new { Do = "baboon" }).DoSomething(66).baboon(Dx.Callback(() => bag.v3 = true)).Returns(77);
                })
                .Act(bag =>
                {
                    bag.subject.As<ICurrentTest>().DoSomething(22);
                    bag.subject.As<ICurrentTest>().DoSomething(44);
                })
                .Assert((bag) =>
                {
                    Assert.True(bag.v1);
                    Assert.True(bag.v2);
                    Assert.False(bag.v3);
                })
                .Run();
        }

        [Test]
        public void As()
        {
            Dx.Test("")
                .Arrange(bag => bag.subject.DoSomething(22).Returns(44))
                .Act(bag =>
                {
                    bag.v1 = bag.subject(new MockSettings { As = "baboon" }).baboon<ICurrentTest>().DoSomething(22);
                    bag.v2 = bag.subject(new { As = "baboon" }).baboon<ICurrentTest>().DoSomething(22);
                })
                .Assert((bag) =>
                {
                    Assert.AreEqual(bag.v2, 44);
                    Assert.AreEqual(bag.v1, 44);
                })
                .Run();
        }

        [Test]
        public void Constructor()
        {
            // See ConstructorArgs class
            Assert.Pass();
        }
    }
}
