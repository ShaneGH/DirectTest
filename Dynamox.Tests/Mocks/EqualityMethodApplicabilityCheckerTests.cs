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
    public class EqualityMethodApplicabilityCheckerTests
    {
        [Test]
        public void TestArgs_Success()
        {
            // arrange
            var arg1 = 999;
            var arg2 = "asdasd";
            object arg3 = null;
            var arg4 = new object();

            var subject = new EqualityMethodApplicabilityChecker(new object[] { arg1, arg2, arg3, arg4 });

            // act, assert
            Assert.IsTrue(subject.TestArgs(new object[] { arg1, arg2, arg3, arg4 }));
        }

        [Test]
        public void TestArgs_Fail_WrongType()
        {
            // arrange
            var arg1 = 999;
            var arg2 = "asdasd";
            object arg3 = null;
            var arg4 = new object();

            var subject = new EqualityMethodApplicabilityChecker(new object[] { arg1, 555, arg3, arg4 });

            // act, assert
            Assert.IsFalse(subject.TestArgs(new object[] { arg1, arg2, arg3, arg4 }));
        }

        [Test]
        public void TestArgs_Fail_WrongValue_ValueType()
        {
            // arrange
            var arg1 = 999;
            var arg2 = "asdasd";
            object arg3 = null;
            var arg4 = new object();

            var subject = new EqualityMethodApplicabilityChecker(new object[] { arg1 + 1, arg2, arg3, arg4 });

            // act, assert
            Assert.IsFalse(subject.TestArgs(new object[] { arg1, arg2, arg3, arg4 }));
        }

        [Test]
        public void TestArgs_Fail_WrongValue_RefType()
        {
            // arrange
            var arg1 = 999;
            var arg2 = "asdasd";
            object arg3 = null;
            var arg4 = new object();

            var subject = new EqualityMethodApplicabilityChecker(new object[] { arg1, arg2, arg3, new object() });

            // act, assert
            Assert.IsFalse(subject.TestArgs(new object[] { arg1, arg2, arg3, arg4 }));
        }

        [Test]
        public void TestArgs_Nulled()
        {
            // arrange
            var arg1 = 999;
            var arg2 = "asdasd";
            object arg3 = null;
            var arg4 = new object();

            var subject = new EqualityMethodApplicabilityChecker(new object[] { arg1, arg2, arg3, null });

            // act, assert
            Assert.IsFalse(subject.TestArgs(new object[] { arg1, arg2, arg3, arg4 }));
        }

        [Test]
        public void TestArgs_NonNulled()
        {
            // arrange
            var arg1 = 999;
            var arg2 = "asdasd";
            object arg3 = null;
            var arg4 = new object();

            var subject = new EqualityMethodApplicabilityChecker(new object[] { arg1, arg2, new object(), arg4 });

            // act, assert
            Assert.IsFalse(subject.TestArgs(new object[] { arg1, arg2, arg3, arg4 }));
        }
    }
}
