using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Dynamox.Tests.Features
{
    [TestFixture]
    public class TestSetup
    {
        static void AreLinear(params DateTime[] dates)
        {
            for (var i = 1; i < dates.Length; i++)
                Assert.Less(dates[i - 1], dates[i]);
        }

        static readonly object Lock = new object();
        static DateTime UniqueDateTime()
        {
            lock (Lock)
            {
                Thread.Sleep(1);
                return DateTime.Now;
            }
        }

        [Test]
        public void BasicFunctionality()
        {
            DateTime? point1 = null, point2 = null, point3 = null;
            Dx.Test("")
                .Arrange(a => point1 = UniqueDateTime())
                .Act(a => point2 = UniqueDateTime())
                .Assert(a => point3 = UniqueDateTime())
                .Run();

            Assert.IsTrue(point1.HasValue);
            Assert.IsTrue(point2.HasValue);
            Assert.IsTrue(point3.HasValue);

            AreLinear(point1.Value, point2.Value, point3.Value);
        }

        [Test]
        public void BaseTest()
        {
            DateTime? point1 = null, point2 = null, point3 = null, point4 = null, point5 = null, point6 = null;

            var module = Dx.Module("bla");
            module.Add("t1")
                .Arrange(a => point1 = UniqueDateTime())
                .Act(a => point3 = UniqueDateTime())
                .Assert(a => point5 = UniqueDateTime());

            module.Add("t2")
                .BasedOn("t1")
                //TODO: skip parent arrange
                .Arrange(a => point2 = UniqueDateTime())
                .UseParentAct(true)
                .Act(a => point4 = UniqueDateTime())
                .SkipParentAssert(false)
                .Assert(a => point6 = UniqueDateTime());

            Dx.Run(module, "t2");

            Assert.IsTrue(point1.HasValue);
            Assert.IsTrue(point2.HasValue);
            Assert.IsTrue(point3.HasValue);
            Assert.IsTrue(point4.HasValue);
            Assert.IsTrue(point5.HasValue);
            Assert.IsTrue(point6.HasValue);

            AreLinear(point1.Value, point2.Value, point3.Value, point4.Value, point5.Value, point6.Value);
        }

        [Test]
        public void DefaultBaseTestUsage()
        {
            DateTime? point1 = null, point2 = null, point3 = null, point4 = null, point5 = null, point6 = null;

            var module = Dx.Module("bla");
            module.Add("t1")
                .Arrange(a => point1 = UniqueDateTime())
                .Act(a => point3 = UniqueDateTime())
                .Assert(a => point6 = UniqueDateTime());

            module.Add("t2")
                .BasedOn("t1")
                .Arrange(a => point2 = UniqueDateTime())
                .Act(a => point4 = UniqueDateTime())
                .Assert(a => point6 = UniqueDateTime());

            Dx.Run(module, "t2");

            Assert.IsTrue(point1.HasValue);
            Assert.IsTrue(point2.HasValue);
            Assert.IsTrue(point3.HasValue);
            Assert.IsTrue(point4.HasValue);
            Assert.IsFalse(point5.HasValue);
            Assert.IsTrue(point6.HasValue);

            AreLinear(point1.Value, point2.Value, point3.Value, point4.Value, point6.Value);
        }

        [Test]
        public void IgnoreBaseTest()
        {
            DateTime? point1 = null, point2 = null, point3 = null, point4 = null, point5 = null, point6 = null;

            var module = Dx.Module("bla");
            module.Add("t1")
                .Arrange(a => point1 = UniqueDateTime())
                .Act(a => point3 = UniqueDateTime())
                .Assert(a => point5 = UniqueDateTime());

            module.Add("t2")
                .BasedOn("t1")
                //TODO: skip parent arrange
                .Arrange(a => point2 = UniqueDateTime())
                .UseParentAct(false)
                .Act(a => point4 = UniqueDateTime())
                .SkipParentAssert(true)
                .Assert(a => point6 = UniqueDateTime());

            Dx.Run(module, "t2");

            Assert.IsTrue(point1.HasValue);
            Assert.IsTrue(point2.HasValue);
            Assert.IsFalse(point3.HasValue);
            Assert.IsTrue(point4.HasValue);
            Assert.IsFalse(point5.HasValue);
            Assert.IsTrue(point6.HasValue);

            AreLinear(point1.Value, point2.Value, point4.Value, point6.Value);
        }

        public class MyClass
        {
            readonly int Val;

            public MyClass (int val)
            {
                Val = val;
            }

            public int Add(int val)
            {
                return val + Val;
            }
        }

        [Test]
        public void ConstructorMethodSyntax1()
        {
            bool ok = false;
            Dx.Test("bla")
                .Subject(typeof(MyClass).GetConstructors()[0])
                .For(typeof(MyClass).GetMethod("Add"))
                .Arrange(bag =>
                {
                    bag.CArgs.val = 2;
                    bag.Args.val = 3;
                })
                .Assert((bag, output) =>
                {
                    Assert.IsTrue(output.Equals(5));
                    ok = true;
                })
                .Run();

            Assert.IsTrue(ok);
        }

        [Test]
        public void ConstructorMethodSyntax2()
        {
            bool ok = false;
            Dx.Test("bla")
                .Subject(() => new MyClass(0))
                .For(a => a.Add(0))
                .Arrange(bag =>
                {
                    bag.CArgs.val = 2;
                    bag.Args.val = 3;
                })
                .Assert((bag, output) =>
                {
                    Assert.AreEqual(output, 5);
                    ok = true;
                })
                .Run();

            Assert.IsTrue(ok);
        }

        [Test]
        public void Throws()
        {
            bool ok = false;
            Dx.Test("bla")
                .Arrange(bag =>
                {
                    bag.CArgs.val = 2;
                    bag.Args.val = 3;
                })
                .Act(bag =>
                {
                    throw new InvalidOperationException();
                })

                .SkipParentThrows()
                .Throws<Exception>((bag, ex) =>
                {
                    Assert.IsInstanceOf<InvalidOperationException>(ex);
                    ok = true;
                })
                .Run();

            Assert.IsTrue(ok);
        }

        [Test]
        public void Throws_WrongExceptionType()
        {
            var test = Dx.Test("bla")
                .Arrange(bag =>
                {
                    bag.CArgs.val = 2;
                    bag.Args.val = 3;
                })
                .Act(bag =>
                {
                    throw new FormatException();
                })

                .SkipParentThrows()
                .Throws<EncoderFallbackException>();

            Assert.Throws<InvalidOperationException>(() => test.Run());
        }

        [Test]
        public void BaseTest_Throws_Default()
        {
            DateTime? point1 = null, point2 = null;

            var module = Dx.Module("bla");
            module.Add("t1")
                .Arrange(a => { })
                .Act(a => { })
                .Throws<InvalidOperationException>((a, b) => point1 = UniqueDateTime());

            module.Add("t2")
                .BasedOn("t1")
                .Arrange(a => { })
                .UseParentAct(false)
                .Act(a => { throw new InvalidOperationException(); })
                .Throws<InvalidOperationException>((a, b) => point2 = UniqueDateTime());

            Dx.Run(module, "t2");

            Assert.IsFalse(point1.HasValue);
            Assert.IsTrue(point2.HasValue);
        }

        [Test]
        public void BaseTestUsage_Throws_DoUseParent()
        {
            DateTime? point1 = null, point2 = null;

            var module = Dx.Module("bla");
            module.Add("t1")
                .Arrange(a => { })
                .Act(a => { })
                .Throws<InvalidOperationException>((a, b) => point1 = UniqueDateTime());

            module.Add("t2")
                .BasedOn("t1")
                .Arrange(a => { })
                .UseParentAct(false)
                .Act(a => { throw new InvalidOperationException(); })
                .SkipParentThrows(false)
                .Throws<InvalidOperationException>((a, b) => point2 = UniqueDateTime());

            Dx.Run(module, "t2");

            Assert.IsTrue(point1.HasValue);
            Assert.IsTrue(point2.HasValue);

            AreLinear(point1.Value, point2.Value);
        }

        [Test]
        public void IgnoreBaseTest_Throws()
        {
            DateTime? point1 = null, point2 = null;

            var module = Dx.Module("bla");
            module.Add("t1")
                .Arrange(a => { })
                .Act(a => { })
                .Throws<InvalidOperationException>((a, b) => point1 = UniqueDateTime());

            module.Add("t2")
                .BasedOn("t1")
                .Arrange(a => { })
                .UseParentAct(false)
                .Act(a => { throw new InvalidOperationException(); })
                .SkipParentThrows()
                .Throws<InvalidOperationException>((a, b) => point2 = UniqueDateTime());

            Dx.Run(module, "t2");

            Assert.IsFalse(point1.HasValue);
            Assert.IsTrue(point2.HasValue);
        }
    }
}
