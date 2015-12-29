using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Compile;
using Dynamox.Mocks;
using Dynamox.Mocks.Info;
using NUnit.Framework;
using COMPILER = Dynamox.Compile.Compiler;

namespace Dynamox.Tests.Compile.Compiler
{
    [TestFixture]
    public class Compiler_InterfacesAndClasses
    {
        public interface IInterfaceAndClass
        {
            int Prop { get; set; }
            int Method();
        }

        public class InterfaceAndClass1
        {
            public virtual int Prop
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public virtual int Method()
            {
                throw new NotImplementedException();
            }
        }

        public class InterfaceAndClass2 : InterfaceAndClass1, IInterfaceAndClass
        {
        }

        [Test]
        public void InterfaceAndClassTests()
        {
            dynamic builder = new MockBuilder(Dx.Settings);
            builder.Prop = 77;
            builder.Method().Returns(88);

            var subject = (InterfaceAndClass2)COMPILER.Compile(typeof(InterfaceAndClass2))
                    .GetConstructors()[0]
                        .Invoke(new object[] { new ObjectBase(Dx.Settings, builder.Values) });

            Assert.AreEqual(subject.Prop, 77);
            Assert.AreEqual(subject.Method(), 88);
        }

        public interface IExplicitInterface
        {
            int Prop { get; set; }
            int Method();
        }

        public class ExplicitInterface : IExplicitInterface
        {
            public virtual int Prop
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public virtual int Method()
            {
                throw new NotImplementedException();
            }

            int IExplicitInterface.Prop
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            int IExplicitInterface.Method()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void ExplicitInterfaceTests()
        {
            dynamic builder = new MockBuilder(Dx.Settings);
            builder.Prop = 77;
            builder.Method().Returns(88);

            var subject = (ExplicitInterface)COMPILER.Compile(typeof(ExplicitInterface))
                    .GetConstructors()[0]
                        .Invoke(new object[] { new ObjectBase(Dx.Settings, builder.Values) });

            Assert.AreEqual(subject.Prop, 77);
            Assert.AreEqual(subject.Method(), 88);
            Assert.AreEqual(((IExplicitInterface)subject).Prop, 77);
            Assert.AreEqual(((IExplicitInterface)subject).Method(), 88);
        }
    }
}