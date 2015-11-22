using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectTests.Mocks;
using NUnit.Framework;

namespace DirectTests.Tests.Mocks
{
    [TestFixture]
    public class MethodApplicabilityCheckerTests
    {
        [Test]
        public void TestArgs_Vanilla()
        {
            var subject = new MethodApplicabilityChecker();

            Assert.IsTrue(subject.TestArgs());
            Assert.IsTrue(subject.TestArgs(Enumerable.Empty<object>()));
            Assert.IsFalse(subject.TestArgs(new object[] { new object() }));
            Assert.IsFalse(subject.TestArgs(new object[] { null }));
        }
    }
}
