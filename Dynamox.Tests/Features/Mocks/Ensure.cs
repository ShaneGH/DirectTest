﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;
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
                .Arrange(bag => bag.subject.DoSomething("Hello").DxEnsure())
                .Act(bag => { ((ICurrentTest)bag.subject.DxAs<ICurrentTest>()).DoSomething("Hello"); })
                .Run();
        }

        [Test]
        public void Method_NotOk()
        {
            var test = Dx.Test("")
                .Arrange(bag => bag.subject.DoSomething("Hello").DxEnsure())
                .Act(bag => { ((ICurrentTest)bag.subject.DxAs<ICurrentTest>()).DoSomething("Not hello"); });

            Assert.Throws<InvalidOperationException>(() => test.Run());
        }

        [Test]
        public void Method_Ok_Deep()
        {
            Dx.Test("")
                .Arrange(bag => bag.subject.GetAnother().DxEnsure().DoSomething("Hello").DxEnsure())
                .Act(bag => { ((ICurrentTest)bag.subject.DxAs<ICurrentTest>()).GetAnother().DoSomething("Hello"); })
                .Run();
        }

        [Test]
        public void Method_NotOk_Deep()
        {
            var test = Dx.Test("")
                .Arrange(bag => bag.subject.GetAnother().DoSomething("Hello").DxEnsure())
                .Act(bag => { ((ICurrentTest)bag.subject.DxAs<ICurrentTest>()).GetAnother().DoSomething("Not hello"); });

            Assert.Throws<InvalidOperationException>(() => test.Run());
        }

        [Test]
        public void Method_Ok_AfterProperty()
        {
            Dx.Test("")
                .Arrange(bag => bag.subject.Another.DoSomething("Hello").DxEnsure())
                .Act(bag => { ((ICurrentTest)bag.subject.DxAs<ICurrentTest>()).Another.DoSomething("Hello"); })
                .Run();
        }

        [Test]
        public void Strong_Method_OK()
        {
            var mock1 = Dx.Strong<ICurrentTest>();
            mock1.Mock(m => m.DoSomething("hello1")).DxEnsure();
            Assert.Throws<MockedMethodNotCalledException>(() => Dx.Ensure(mock1));

            mock1.DxAs().DoSomething("hello1");
            Dx.Ensure(mock1);
        }

        [Test]
        public void Property_OK()
        {
            var mock1 = Dx.Mock();
            mock1.Another = Dx.EnsuredProperty(null);
            Assert.Throws<MockedMethodNotCalledException>(() => Dx.Ensure(mock1));

            var val = ((ICurrentTest)mock1.DxAs<ICurrentTest>()).Another;
            Dx.Ensure(mock1);
        }

        [Test]
        public void Nested_Property_OK()
        {
            var mock1 = Dx.Mock();
            mock1.Another.Another = Dx.EnsuredProperty(null);
            Assert.Throws<MockedMethodNotCalledException>(() => Dx.Ensure(mock1));

            var val = ((ICurrentTest)mock1.DxAs<ICurrentTest>()).Another.Another;
            Dx.Ensure(mock1);
        }

        [Test]
        public void Strong_Object_Ok()
        {
            var mock1 = Dx.Strong<ICurrentTest>(m => m.Mock(x => x.DoSomething("hello1")).DxEnsure());
            Assert.Throws<MockedMethodNotCalledException>(() => Dx.Ensure(mock1));

            mock1.DoSomething("hello1");
            Dx.Ensure(mock1);
        }

        [Test]
        public void Method_NotOk_AfterProperty()
        {
            var test = Dx.Test("")
                .Arrange(bag => bag.subject.Another.DoSomething("Hello").DxEnsure())
                .Act(bag => { ((ICurrentTest)bag.subject.DxAs<ICurrentTest>()).Another.DoSomething("Not hello"); });

            Assert.Throws<InvalidOperationException>(() => test.Run());
        }

        [Test]
        public void DxDotEnsure()
        {
            var mock1 = Dx.Mock();
            var mock2 = Dx.Mock();
            mock1.DoSomething("hello1").DxEnsure();
            mock2.DoSomething("hello2").DxEnsure();
            Assert.Throws<MockedMethodNotCalledException>(() => Dx.Ensure(mock1, mock2));

            ((ICurrentTest)mock1.DxAs<ICurrentTest>()).DoSomething("hello");
            ((ICurrentTest)mock2.DxAs<ICurrentTest>()).DoSomething("hello");
            Assert.Throws<MockedMethodNotCalledException>(() => Dx.Ensure(mock1, mock2));

            ((ICurrentTest)mock1.DxAs<ICurrentTest>()).DoSomething("hello1");
            ((ICurrentTest)mock2.DxAs<ICurrentTest>()).DoSomething("hello2");
            Dx.Ensure(mock1, mock2);
        }
    }
}