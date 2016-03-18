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
    public class Properties
    {
        public interface ICurrentTest
        {
            int BeSomething { get; set; }
        }

        [Test]
        public void DynamicPropertyVal_CannotSet()
        {
            var v = 55;
            Dx.Test("")
                .Arrange(bag => bag.subject.BeSomething = Dx.Property<int>(() => v++))
                .Act(bag =>
                {
                    bag.v1 = ((ICurrentTest)bag.subject.DxAs<ICurrentTest>()).BeSomething;
                    bag.v2 = ((ICurrentTest)bag.subject.DxAs<ICurrentTest>()).BeSomething;
                    ((ICurrentTest)bag.subject.DxAs<ICurrentTest>()).BeSomething = 99;
                    bag.v3 = ((ICurrentTest)bag.subject.DxAs<ICurrentTest>()).BeSomething;
                })
                .Assert((bag) =>
                {
                    Assert.AreEqual(bag.v1, 55);
                    Assert.AreEqual(bag.v2, 56);
                    Assert.AreEqual(bag.v3, 57);
                })
                .Run();
        }

        [Test]
        public void DynamicPropertyVal_CanSet()
        {
            var v = 55;
            Dx.Test("")
                .Arrange(bag => bag.subject.BeSomething = Dx.Property<int>(() => v++, true))
                .Act(bag =>
                {
                    bag.v1 = ((ICurrentTest)bag.subject.DxAs<ICurrentTest>()).BeSomething;
                    bag.v2 = ((ICurrentTest)bag.subject.DxAs<ICurrentTest>()).BeSomething;
                    ((ICurrentTest)bag.subject.DxAs<ICurrentTest>()).BeSomething = 99;
                    bag.v3 = ((ICurrentTest)bag.subject.DxAs<ICurrentTest>()).BeSomething;
                })
                .Assert((bag) =>
                {
                    Assert.AreEqual(bag.v1, 55);
                    Assert.AreEqual(bag.v2, 56);
                    Assert.AreEqual(bag.v3, 99);
                })
                .Run();
        }

        [Test]
        public void DynamicPropertyVal_Callback()
        {
            Dx.Test("")
                .Arrange(bag => bag.subject.BeSomething =
                    Dx.Property<int>(33)
                        .OnGet(a =>
                        {
                            bag.ok1 = true;
                            Assert.AreEqual(a, 33);
                        })
                        .OnSet((a, b) =>
                        {
                            bag.ok2 = true;
                            Assert.AreEqual(a, 33);
                            Assert.AreEqual(b, 44);
                        }))
                .Act(bag =>
                {
                    var x = ((ICurrentTest)bag.subject.DxAs<ICurrentTest>()).BeSomething;
                    ((ICurrentTest)bag.subject.DxAs<ICurrentTest>()).BeSomething = 44;
                })
                .Assert((bag) =>
                {
                    Assert.IsTrue(bag.ok1);
                    Assert.IsTrue(bag.ok2);
                })
                .Run();
        }
    }
}