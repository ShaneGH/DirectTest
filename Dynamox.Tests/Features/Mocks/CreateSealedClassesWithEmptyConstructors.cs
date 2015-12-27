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
    public class CreateSealedClasses
    {
        public sealed class C1 { public string Prop { get; set; } }
        public class C2 { public C1 Prop { get; set; } }

        [Test]
        public void Create()
        {
            Dx.Test("")
                .Arrange(bag => bag.subject.Prop.Prop = "Hello")
                .Act(bag => (string)bag.subject.As<C2>().Prop.Prop)
                .Assert((a, b) => Assert.AreEqual("Hello", b))
                .Run();
        }

        [Test]
        public void DoNotCreate()
        {
            Dx.Test("", new DxSettings { CreateSealedClasses = false })
                .Arrange(bag => bag.subject.Prop.Prop = "Hello")
                .Act(bag => (string)bag.subject.As<C2>().Prop.Prop)
                .Throws<Exception>()
                .Run();
        }
    }
}