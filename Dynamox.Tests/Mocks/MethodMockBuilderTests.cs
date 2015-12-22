using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Dynamox.Mocks
{
    [TestFixture]
    internal class MethodMockBuilderTests
    {
        public class C0 { }
        public class C1 : C0 { }
        public class C2 : C1 { }

        public static void TestMethod<T1, T2>(T1 arg1, int arg2)
            where T1 : C1
        { }

        [Test]
        public void RepresentsMethodTest_OK1_ConstructedGeneric()
        {
            // arrange
            var method = typeof(MethodMockBuilderTests).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == "TestMethod")
                .MakeGenericMethod(new[] { typeof(C1), typeof(double) });
            var builder = new MethodMockBuilder(null, new[] { typeof(C1), typeof(double) }, new object[] { new C2(), 5 });

            // act
            // assert
            Assert.True(builder.RepresentsMethod(method));
        }

        [Test]
        public void RepresentsMethodTest_Not_OK1_ConstructedGeneric()
        {
            // arrange
            var method = typeof(MethodMockBuilderTests).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == "TestMethod")
                .MakeGenericMethod(new[] { typeof(C1), typeof(double) });
            var builder = new MethodMockBuilder(null, new[] { typeof(C2), typeof(double) }, new object[] { new C2(), 5 });

            // act
            // assert
            Assert.False(builder.RepresentsMethod(method));
        }

        [Test]
        public void RepresentsMethodTest_Not_OK2_ConstructedGeneric()
        {
            // arrange
            var method = typeof(MethodMockBuilderTests).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == "TestMethod")
                .MakeGenericMethod(new[] { typeof(C1), typeof(double) });
            var builder = new MethodMockBuilder(null, new[] { typeof(C1), typeof(double) }, new object[] { new C0(), 5 });

            // act
            // assert
            Assert.False(builder.RepresentsMethod(method));
        }

        [Test]
        public void RepresentsMethodTest_OK1_Generic()
        {
            // arrange
            var method = typeof(MethodMockBuilderTests).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == "TestMethod");
            var builder = new MethodMockBuilder(null, new[] { typeof(C1), typeof(double) }, new object[] { new C2(), 5 });

            // act
            // assert
            Assert.True(builder.RepresentsMethod(method));
        }

        [Test]
        public void RepresentsMethodTest_OK2_Generic()
        {
            // arrange
            var method = typeof(MethodMockBuilderTests).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == "TestMethod");
            var builder = new MethodMockBuilder(null, new[] { typeof(C2), typeof(double) }, new object[] { new C2(), 5 });

            // act
            // assert
            Assert.True(builder.RepresentsMethod(method));
        }

        [Test]
        public void RepresentsMethodTest_NotOK_Generic()
        {
            // arrange
            var method = typeof(MethodMockBuilderTests).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == "TestMethod");
            var builder = new MethodMockBuilder(null, new[] { typeof(C0), typeof(double) }, new object[] { new C2(), 5 });

            // act
            // assert
            Assert.True(builder.RepresentsMethod(method));
        }
    }
}