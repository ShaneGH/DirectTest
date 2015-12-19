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
    public class Any
    {
        public interface ICurrentTest
        {
            void DoSomething(string val);
            void DoSomething(int val);
        }

        [Test]
        public void ReferenceType()
        {
            Dx.Test("")
                .Arrange(bag => bag.subject.DoSomething(Dx.Any).Ensure())
                .Act(bag => { bag.subject.As<ICurrentTest>().DoSomething("Hello"); })
                .Run();
        }

        [Test]
        public void ValueType()
        {
            Dx.Test("")
                .Arrange(bag => bag.subject.DoSomething(Dx.Any).Ensure())
                .Act(bag => { bag.subject.As<ICurrentTest>().DoSomething(4); })
                .Run();
        }

        [Test]
        public void Null()
        {
            Dx.Test("")
                .Arrange(bag => bag.subject.DoSomething(Dx.Any).Ensure())
                .Act(bag => { bag.subject.As<ICurrentTest>().DoSomething(null); })
                .Run();
        }
    }
}