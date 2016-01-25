using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Dynamox.Tests.Features.Mocks
{
    [TestFixture]
    public class Events
    {
        public interface ICurrentTest
        {
            event EventHandler SomethingDid;
            event EventHandler SomethingDid_Mock;
            event EventHandler SomethingDid_Object;
            event EventHandler SomethingDid_Not;
        }

        public abstract class AbstractClass : ICurrentTest
        {
            public abstract event EventHandler SomethingDid;
            public abstract event EventHandler SomethingDid_Mock;
            public abstract event EventHandler SomethingDid_Object;
            public abstract event EventHandler SomethingDid_Not;
        }

        public abstract class VirtualClass : ICurrentTest
        {
            public virtual event EventHandler SomethingDid;
            public virtual event EventHandler SomethingDid_Mock;
            public virtual event EventHandler SomethingDid_Object;
            public virtual event EventHandler SomethingDid_Not;
        }

        public abstract class ChildClass : VirtualClass
        {
            public override event EventHandler SomethingDid;
        }

        [Test]
        public void AbstractClassTest()
        {
            Test<AbstractClass>();
        }

        [Test]
        public void VirtualClassTest()
        {
            Test<VirtualClass>();
        }

        [Test]
        public void ChildClassTest()
        {
            Test<ChildClass>();
        }

        [Test]
        public void InterfaceTest()
        {
            Test<ICurrentTest>();
        }

        void Test<T>()
            where T : ICurrentTest
        {
            var args = new object[] { new object(), new EventArgs() };
            var mock = Dx.Mock();

            int raised1 = 0, raised2 = 0;
            mock.SomethingDid += Dx.EventHandler<object, EventArgs>((sender, e) =>
            {
                Assert.AreEqual(sender, args[0]);
                Assert.AreEqual(e, args[1]);

                raised1++;
            });
            mock.SomethingDid_Mock += Dx.EventHandler<object, EventArgs>((sender, e) =>
            {
                Assert.AreEqual(sender, args[0]);
                Assert.AreEqual(e, args[1]);

                raised1++;
            });

            T test = mock.As<T>();

            test.SomethingDid += (sender, e) =>
            {
                Assert.AreEqual(sender, args[0]);
                Assert.AreEqual(e, args[1]);

                raised2++;
            };
            test.SomethingDid_Object += (sender, e) =>
            {
                Assert.AreEqual(sender, args[0]);
                Assert.AreEqual(e, args[1]);

                raised2++;
            };

            // raise on mock
            Assert.True(Dx.RaiseEvent(mock, "SomethingDid", args));
            Assert.AreEqual(1, raised1);
            Assert.AreEqual(1, raised2);
            Assert.True(Dx.RaiseEvent(mock, "SomethingDid_Mock", args));
            Assert.AreEqual(2, raised1);
            Assert.AreEqual(1, raised2);
            Assert.True(Dx.RaiseEvent(mock, "SomethingDid_Object", args));
            Assert.AreEqual(2, raised1);
            Assert.AreEqual(2, raised2);
            Assert.False(Dx.RaiseEvent(mock, "SomethingDidNot", args));
            Assert.AreEqual(2, raised1);
            Assert.AreEqual(2, raised2);

            // raise on object
            Assert.True(Dx.RaiseEvent(test, "SomethingDid", args));
            Assert.AreEqual(3, raised1);
            Assert.AreEqual(3, raised2);
            Assert.True(Dx.RaiseEvent(test, "SomethingDid_Mock", args));
            Assert.AreEqual(4, raised1);
            Assert.AreEqual(3, raised2);
            Assert.True(Dx.RaiseEvent(test, "SomethingDid_Object", args));
            Assert.AreEqual(4, raised1);
            Assert.AreEqual(4, raised2);
            Assert.False(Dx.RaiseEvent(test, "SomethingDidNot", args));
            Assert.AreEqual(4, raised1);
            Assert.AreEqual(4, raised2);
        }
    }
}