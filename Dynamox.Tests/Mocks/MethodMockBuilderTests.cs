using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Dynamox.Mocks
{
    [TestFixture]
    internal class MethodMockBuilderTests
    {
        public static void TestMethod<T1, T2>(T1 arg1, int arg2) { }

        [Test]
        public void RepresentsMethodTest_OK()
        {
            // arrange
            var method = typeof(MethodMockBuilderTests).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == "TestMethod")
                .MakeGenericMethod(new[] { typeof(string), typeof(double) });
            var builder = new MethodMockBuilder(null, new[] { typeof(string), typeof(double) }, new object[] { "asdasd", 5 });

            // act
            // assert
            Assert.True(builder.RepresentsMethod(method));
        }

        [Test]
        public void RepresentsMethodTest_Not_OK1()
        {
            // arrange
            var method = typeof(MethodMockBuilderTests).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == "TestMethod")
                .MakeGenericMethod(new[] { typeof(string), typeof(double) });
            var builder = new MethodMockBuilder(null, new[] { typeof(string), typeof(int) }, new object[] { "asdasd", 5 });

            // act
            // assert
            Assert.False(builder.RepresentsMethod(method));
        }

        [Test]
        public void RepresentsMethodTest_Not_OK2()
        {
            // arrange
            var method = typeof(MethodMockBuilderTests).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == "TestMethod")
                .MakeGenericMethod(new[] { typeof(string), typeof(double) });
            var builder = new MethodMockBuilder(null, new[] { typeof(string), typeof(double) }, new object[] { 9, 5 });

            // act
            // assert
            Assert.False(builder.RepresentsMethod(method));
        }

        //public bool RepresentsMethod(MethodInfo method)
        //{
        //    //TODO: generic constraints???
        //    if (GenericArguments.Count() != method.GetGenericArguments().Length)
        //    {
        //        // TODO: reason
        //        return false;
        //    }

        //    //TODO: out params
        //    return ArgChecker.TestArgTypes(method.GetParameters().Select(p => p.ParameterType));
        //}
    }
}
