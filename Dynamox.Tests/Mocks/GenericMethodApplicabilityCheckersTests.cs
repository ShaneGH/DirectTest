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
    public class GenericMethodApplicabilityCheckersTests
    {
        [Test]
        public void TestArgs_ParentType()
        {
            // arrange
            var arg1 = "asdasdasd";

            var subject = new MethodApplicabilityChecker<object>(a =>
            {
                return a.Equals(arg1);
            });

            // act, assert
            Assert.IsTrue(subject.TestArgs(new object[] { arg1 }));
        }

        [Test]
        public void TestArgs_ChildType()
        {
            // arrange
            var arg1 = new object();

            var subject = new MethodApplicabilityChecker<string>(a =>
            {
                return a.Equals(arg1);
            });

            // act, assert
            Assert.IsFalse(subject.TestArgs(new object[] { arg1 }));
        }

        [Test]
        public void TestArgs_1_Arg()
        {
            // arrange
            var arg1 = 999;
            var result = true;
            var amalgamated = new object[] { arg1 };

            var subject = new MethodApplicabilityChecker<int>(a =>
            {
                Assert.AreEqual(a, arg1);
                return result;
            });

            // act, assert
            Assert.IsTrue(subject.TestArgs(amalgamated));
            Assert.IsFalse(subject.TestArgs(amalgamated.Skip(1)));
            Assert.IsFalse(subject.TestArgs(amalgamated.Take(amalgamated.Length - 1)));
            Assert.IsFalse(subject.TestArgs(amalgamated.Concat(new[] { new object() })));
            result = false;
            Assert.IsFalse(subject.TestArgs(amalgamated));
        }

        [Test]
        public void TestArgs_2_Arg()
        {
            // arrange
            var arg1 = 999;
            var arg2 = "asdasd";
            var result = true;
            var amalgamated = new object[] { arg1, arg2 };

            var subject = new MethodApplicabilityChecker<int, string>((a, b) =>
            {
                Assert.AreEqual(a, arg1);
                Assert.AreEqual(b, arg2);
                return result;
            });

            // act, assert
            Assert.IsTrue(subject.TestArgs(amalgamated));
            Assert.IsFalse(subject.TestArgs(amalgamated.Skip(1)));
            Assert.IsFalse(subject.TestArgs(amalgamated.Take(amalgamated.Length - 1)));
            Assert.IsFalse(subject.TestArgs(amalgamated.Concat(new[] { new object() })));
            result = false;
            Assert.IsFalse(subject.TestArgs(amalgamated));
        }

        [Test]
        public void TestArgs_3_Arg()
        {
            // arrange
            var arg1 = 999;
            var arg2 = "asdasd";
            object arg3 = null;
            var result = true;
            var amalgamated = new object[] { arg1, arg2, arg3 };

            var subject = new MethodApplicabilityChecker<int, string, object>((a, b, c) =>
            {
                Assert.AreEqual(a, arg1);
                Assert.AreEqual(b, arg2);
                Assert.AreEqual(c, arg3);
                return result;
            });

            // act, assert
            Assert.IsTrue(subject.TestArgs(amalgamated));
            Assert.IsFalse(subject.TestArgs(amalgamated.Skip(1)));
            Assert.IsFalse(subject.TestArgs(amalgamated.Take(amalgamated.Length - 1)));
            Assert.IsFalse(subject.TestArgs(amalgamated.Concat(new[] { new object() })));
            result = false;
            Assert.IsFalse(subject.TestArgs(amalgamated));
        }

        [Test]
        public void TestArgs_4_Arg()
        {
            // arrange
            var arg1 = 999;
            var arg2 = "asdasd";
            object arg3 = null;
            var arg4 = new object();
            var result = true;
            var amalgamated = new object[] { arg1, arg2, arg3, arg4 };

            var subject = new MethodApplicabilityChecker<int, string, object, object>((a, b, c, d) =>
            {
                Assert.AreEqual(a, arg1);
                Assert.AreEqual(b, arg2);
                Assert.AreEqual(c, arg3);
                Assert.AreEqual(d, arg4);
                return result;
            });

            // act, assert
            Assert.IsTrue(subject.TestArgs(amalgamated));
            Assert.IsFalse(subject.TestArgs(amalgamated.Skip(1)));
            Assert.IsFalse(subject.TestArgs(amalgamated.Take(amalgamated.Length - 1)));
            Assert.IsFalse(subject.TestArgs(amalgamated.Concat(new[] { new object() })));
            result = false;
            Assert.IsFalse(subject.TestArgs(amalgamated));
        }

        [Test]
        public void TestArgs_5_Arg()
        {
            // arrange
            var arg1 = 999;
            var arg2 = "asdasd";
            object arg3 = null;
            var arg4 = new object();
            var arg5 = 999;
            var result = true;
            var amalgamated = new object[] { arg1, arg2, arg3, arg4, arg5 };

            var subject = new MethodApplicabilityChecker<int, string, object, object, int>((a, b, c, d, e) =>
            {
                Assert.AreEqual(a, arg1);
                Assert.AreEqual(b, arg2);
                Assert.AreEqual(c, arg3);
                Assert.AreEqual(d, arg4);
                Assert.AreEqual(e, arg5);
                return result;
            });

            // act, assert
            Assert.IsTrue(subject.TestArgs(amalgamated));
            Assert.IsFalse(subject.TestArgs(amalgamated.Skip(1)));
            Assert.IsFalse(subject.TestArgs(amalgamated.Take(amalgamated.Length - 1)));
            Assert.IsFalse(subject.TestArgs(amalgamated.Concat(new[] { new object() })));
            result = false;
            Assert.IsFalse(subject.TestArgs(amalgamated));
        }

        [Test]
        public void TestArgs_6_Arg()
        {
            // arrange
            var arg1 = 999;
            var arg2 = "asdasd";
            object arg3 = null;
            var arg4 = new object();
            var arg5 = 999;
            var arg6 = "asdasd";
            var result = true;
            var amalgamated = new object[] { arg1, arg2, arg3, arg4, arg5, arg6 };

            var subject = new MethodApplicabilityChecker<int, string, object, object, int, string>((a, b, c, d, e, f) =>
            {
                Assert.AreEqual(a, arg1);
                Assert.AreEqual(b, arg2);
                Assert.AreEqual(c, arg3);
                Assert.AreEqual(d, arg4);
                Assert.AreEqual(e, arg5);
                Assert.AreEqual(f, arg6);
                return result;
            });

            // act, assert
            Assert.IsTrue(subject.TestArgs(amalgamated));
            Assert.IsFalse(subject.TestArgs(amalgamated.Skip(1)));
            Assert.IsFalse(subject.TestArgs(amalgamated.Take(amalgamated.Length - 1)));
            Assert.IsFalse(subject.TestArgs(amalgamated.Concat(new[] { new object() })));
            result = false;
            Assert.IsFalse(subject.TestArgs(amalgamated));
        }









        //[Test]
        //public void TestArgs_Fail_WrongType()
        //{
        //    // arrange
        //    var arg1 = 999;
        //    var arg2 = "asdasd";
        //    object arg3 = null;
        //    var arg4 = new object();

        //    var subject = new EqualityMethodApplicabilityChecker(new object[] { arg1, 555, arg3, arg4 });

        //    // act, assert
        //    Assert.IsFalse(subject.TestArgs(new object[] { arg1, arg2, arg3, arg4 }));
        //}

        //[Test]
        //public void TestArgs_Fail_WrongValue_ValueType()
        //{
        //    // arrange
        //    var arg1 = 999;
        //    var arg2 = "asdasd";
        //    object arg3 = null;
        //    var arg4 = new object();

        //    var subject = new EqualityMethodApplicabilityChecker(new object[] { arg1 + 1, arg2, arg3, arg4 });

        //    // act, assert
        //    Assert.IsFalse(subject.TestArgs(new object[] { arg1, arg2, arg3, arg4 }));
        //}

        //[Test]
        //public void TestArgs_Fail_WrongValue_RefType()
        //{
        //    // arrange
        //    var arg1 = 999;
        //    var arg2 = "asdasd";
        //    object arg3 = null;
        //    var arg4 = new object();

        //    var subject = new EqualityMethodApplicabilityChecker(new object[] { arg1, arg2, arg3, new object() });

        //    // act, assert
        //    Assert.IsFalse(subject.TestArgs(new object[] { arg1, arg2, arg3, arg4 }));
        //}

        //[Test]
        //public void TestArgs_Nulled()
        //{
        //    // arrange
        //    var arg1 = 999;
        //    var arg2 = "asdasd";
        //    object arg3 = null;
        //    var arg4 = new object();

        //    var subject = new EqualityMethodApplicabilityChecker(new object[] { arg1, arg2, arg3, null });

        //    // act, assert
        //    Assert.IsFalse(subject.TestArgs(new object[] { arg1, arg2, arg3, arg4 }));
        //}

        //[Test]
        //public void TestArgs_NonNulled()
        //{
        //    // arrange
        //    var arg1 = 999;
        //    var arg2 = "asdasd";
        //    object arg3 = null;
        //    var arg4 = new object();

        //    var subject = new EqualityMethodApplicabilityChecker(new object[] { arg1, arg2, new object(), arg4 });

        //    // act, assert
        //    Assert.IsFalse(subject.TestArgs(new object[] { arg1, arg2, arg3, arg4 }));
        //}
    }
}
