using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Builders;
using Dynamox.Mocks;

namespace Dynamox
{
    public static class Dx
    {
        public static object Any
        {
            get
            {
                return MethodApplicabilityChecker.Any;
            }
        }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tests"></param>
        /// <param name="singleTest">If not null, only run the given test from the module</param>
        public static void Run(ITestModule tests, string singleTestName = null)
        {
            TestBuilder.Run(tests, singleTestName);
        }

        #region Properties

        public static IPropertyAssertBuilder<TProperty> Property<TProperty>()
        {
            return new PropertyAssertBuilder<TProperty>();
        }

        public static IPropertyAssertBuilder<TProperty> Property<TProperty>(TProperty property)
        {
            return new PropertyAssertBuilder<TProperty>(property);
        }

        public static IPropertyAssertBuilder<TProperty> Property<TProperty>(Func<TProperty> property, bool canSet = false)
        {
            return new PropertyAssertBuilder<TProperty>(property, canSet);
        }

        #endregion

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

        public static IMethodAssert Method<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2, T3, T4, T5, T6, T7>(assert);
        }

        public static IMethodAssert Method<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2, T3, T4, T5, T6, T7, T8>(assert);
        }

        public static IMethodAssert Method<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2, T3, T4, T5, T6, T7, T8, T9>(assert);
        }

        public static IMethodAssert Method<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(assert);
        }

        #endregion

        #region MethodCallback

        public static IMethodCallback Method(Action callback)
        {
            return new MethodCallback(callback);
        }

        public static IMethodCallback Method<T>(Action<T> callback)
        {
            return new MethodCallback<T>(callback);
        }

        public static IMethodCallback Method<T1, T2>(Action<T1, T2> callback)
        {
            return new MethodCallback<T1, T2>(callback);
        }

        public static IMethodCallback Method<T1, T2, T3>(Action<T1, T2, T3> callback)
        {
            return new MethodCallback<T1, T2, T3>(callback);
        }

        public static IMethodCallback Method<T1, T2, T3, T4>(Action<T1, T2, T3, T4> callback)
        {
            return new MethodCallback<T1, T2, T3, T4>(callback);
        }

        public static IMethodCallback Method<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> callback)
        {
            return new MethodCallback<T1, T2, T3, T4, T5>(callback);
        }

        public static IMethodCallback Method<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> callback)
        {
            return new MethodCallback<T1, T2, T3, T4, T5, T6>(callback);
        }

        public static IMethodCallback Method<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> callback)
        {
            return new MethodCallback<T1, T2, T3, T4, T5, T6, T7>(callback);
        }

        public static IMethodCallback Method<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> callback)
        {
            return new MethodCallback<T1, T2, T3, T4, T5, T6, T7, T8>(callback);
        }

        public static IMethodCallback Method<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> callback)
        {
            return new MethodCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9>(callback);
        }

        public static IMethodCallback Method<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> callback)
        {
            return new MethodCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(callback);
        }

        #endregion
    }
}
