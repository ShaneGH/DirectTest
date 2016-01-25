using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Compile;
using Dynamox.Mocks;
using NUnit.Framework;

using COMPILER = Dynamox.Compile.Compiler;

namespace Dynamox.Tests.Compile.Compiler
{
    [TestFixture]
    public class Compiler_Events
    {
        public delegate void ByRefEventHandler(object arg1, string arg2);
        public delegate void ByValEventHandler(bool arg1, int arg2);

        public interface IInterfaceEvents
        {
            event ByRefEventHandler Event1;
            event ByValEventHandler Event2;
        }

        public abstract class AbstractEvents : IInterfaceEvents
        {
            public abstract event ByRefEventHandler Event1;
            public abstract event ByValEventHandler Event2;
        }

        public class VirtualEvents : IInterfaceEvents
        {
            public virtual event ByRefEventHandler Event1;
            public virtual event ByValEventHandler Event2;
        }

        [Test]
        public void SubscribeAndRaiseEvents_Interface()
        {
            SubscribeAndRaiseEvents<IInterfaceEvents>();
        }

        [Test]
        public void SubscribeAndRaiseEvents_Abstract()
        {
            SubscribeAndRaiseEvents<AbstractEvents>();
        }

        [Test]
        public void SubscribeAndRaiseEvents_Virtual()
        {
            SubscribeAndRaiseEvents<VirtualEvents>();
        }

        void SubscribeAndRaiseEvents<T>()
            where T : IInterfaceEvents
        {
            var input1 = "jojobob";
            var input2 = "asdasdasd";
            var input3 = true;
            var input4 = 345435;

            T mock = Dx.Mock().As<T>();
            int total1 = 0, total2 = 0;

            ByRefEventHandler h1 = null;
            mock.Event1 += (h1 = (a, b) =>
            {
                total1++;
                Assert.AreEqual(a, input1);
                Assert.AreEqual(b, input2);
            });

            ByValEventHandler h2 = null;
            mock.Event2 += (h2 = (a, b) =>
            {
                total2++;
                Assert.AreEqual(a, input3);
                Assert.AreEqual(b, input4);
            });

            var ev = mock as IRaiseEvent;
            Assert.True(ev.RaiseEvent("Event1", new[] { input1, input2 }));
            Assert.True(ev.RaiseEvent("Event2", new object[] { input3, input4 }));

            mock.Event1 -= h1;
            mock.Event2 -= h2;

            Assert.True(ev.RaiseEvent("Event1", new[] { input1, input2 }));
            Assert.True(ev.RaiseEvent("Event2", new object[] { input3, input4 }));

            Assert.AreEqual(total1, 1);
            Assert.AreEqual(total2, 1);

            // test for invalid event name
            Assert.False(ev.RaiseEvent("not an event", new[] { input1, input2 }));

            // test for invalid event args
            Assert.Throws<InvalidMockException>(() => ev.RaiseEvent("Event1", new object[0]));
            Assert.Throws<InvalidMockException>(() => ev.RaiseEvent("Event1", new[] { new object(), new object() }));
            Assert.Throws<InvalidMockException>(() => ev.RaiseEvent("Event2", new object[] { true, null }));

            // test for null event args
            Assert.True(ev.RaiseEvent("Event1", new object[] { null, null }));
        }
    }
}