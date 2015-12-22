using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;
using NUnit.Framework;

namespace Dynamox.Tests.Mocks
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

        [Test]
        public void TestArgTypes()
        {
            var subject = new MethodApplicabilityChecker<string, int, object>(null);

            Assert.IsTrue(subject.TestInputArgTypes(new[] { typeof(string), typeof(int), typeof(object) }));
            Assert.IsTrue(subject.TestInputArgTypes(new[] { typeof(string), typeof(int), typeof(string) }));
            Assert.IsFalse(subject.TestInputArgTypes(new[] { typeof(string), typeof(float), typeof(object) }));
        }
    }
}
