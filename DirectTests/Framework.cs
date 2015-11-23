using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectTests.Builders;
using DirectTests.Mocks;

namespace DirectTests
{
    public static class Framework
    {
        public static IArrange Test(string testName)
        {
            return new TestBuilder(testName);
        }

        public static ITestModule Module(string moduleName = null)
        {
            return new TestModule(moduleName);
        }

        public static void Run(ITest test)
        {
            Run(new TestModule(test));
        }

        public static void Run(ITestModule tests)
        {
            TestBuilder.Run(tests);
        }

        #region MethodApplicabilityChecker

        public static IMethodAssert Method()
        {
            return new MethodApplicabilityChecker();
        }

        public static IMethodAssert Method<T>(Func<T, bool> assert)
        {
            return new MethodApplicabilityChecker<T>(assert);
        }

        public static IMethodAssert Method<T1, T2>(Func<T1, T2, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2>(assert);
        }

        public static IMethodAssert Method<T1, T2, T3>(Func<T1, T2, T3, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2, T3>(assert);
        }

        public static IMethodAssert Method<T1, T2, T3, T4>(Func<T1, T2, T3, T4, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2, T3, T4>(assert);
        }

        public static IMethodAssert Method<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2, T3, T4, T5>(assert);
        }

        public static IMethodAssert Method<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2, T3, T4, T5, T6>(assert);
        }

        #endregion
    }
}
