using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Builders;
using Dynamox.Mocks;
using Dynamox.Mocks.Info;

namespace Dynamox
{
    public static class Dx
    {
        public static readonly DxSettings Settings = DxSettings.GlobalSettings;

        public static object Any
        {
            get
            {
                return MethodApplicabilityChecker.Any;
            }
        }

        public static object AnyT<T>()
        {
            return MethodApplicabilityChecker.AnyT<T>();
        }

        public static IArrange Test(string testName, DxSettings settings = null)
        {
            return new TestBuilder(testName, settings ?? new DxSettings());
        }

        public static ITestModule Module(string moduleName = null, DxSettings settings = null)
        {
            return new TestModule(moduleName, settings ?? new DxSettings());
        }

        #region mock

        public static dynamic Mock(IEnumerable<object> constructorArgs = null)
        {
            return new MockBuilder(constructorArgs);
        }

        public static dynamic Mock(DxSettings settings, IEnumerable<object> constructorArgs = null)
        {
            return new MockBuilder(settings, constructorArgs);
        }

        public static dynamic Mock(DxSettings settings, IReservedTerms mockSettings, IEnumerable<object> constructorArgs = null)
        {
            return new MockBuilder(mockSettings, settings, constructorArgs);
        }

        public static dynamic Mock(DxSettings settings, object mockSettings, IEnumerable<object> constructorArgs = null)
        {
            return new MockBuilder(new ReservedTerms(mockSettings), settings, constructorArgs);
        }

        #endregion

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

        /// <summary>
        /// Ensure that all methods mocked by a mock builder and marked with Ensure(...) were called
        /// </summary>
        /// <param name="mockBuilders">The mock builders</param>
        public static void Ensure(params dynamic[] mockBuilders)
        {
            if (!mockBuilders.Select(b => !(b is MockBuilder)).Any())
                throw new InvalidOperationException();  //TODE

            var errors = mockBuilders
                .Select(b => b as MockBuilder)
                .SelectMany(b => b.ShouldHaveBeenCalled);

            if (!errors.Any())
                return;

            throw new InvalidOperationException(string.Join("\n", errors));  //TODE
        }

        #region Properties

        public static IPropertyMockBuilder<TProperty> Property<TProperty>()
        {
            return new PropertyMockBuilder<TProperty>();
        }

        public static IPropertyMockBuilder<TProperty> Property<TProperty>(TProperty property)
        {
            return new PropertyMockBuilder<TProperty>(property);
        }

        public static IPropertyMockBuilder<TProperty> Property<TProperty>(Func<TProperty> property, bool canSet = false)
        {
            return new PropertyMockBuilder<TProperty>(property, canSet);
        }

        #endregion

        #region Indexes

        public static IPropertyMockBuilder<TIndexValue> Index<TIndexValue>()
        {
            return Property<TIndexValue>();
        }

        public static IPropertyMockBuilder<TIndexValue> Index<TIndexValue>(TIndexValue property)
        {
            return Property<TIndexValue>(property);
        }

        public static IPropertyMockBuilder<TIndexValue> Index<TIndexValue>(Func<TIndexValue> property, bool canSet = false)
        {
            return Property<TIndexValue>(property, canSet);
        }

        #endregion

        #region MethodApplicabilityChecker

        public static IMethodAssert Args()
        {
            return new MethodApplicabilityChecker();
        }

        public static IMethodAssert Args<T>(Func<T, bool> assert)
        {
            return new MethodApplicabilityChecker<T>(assert);
        }

        public static IMethodAssert Args<T1, T2>(Func<T1, T2, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2>(assert);
        }

        public static IMethodAssert Args<T1, T2, T3>(Func<T1, T2, T3, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2, T3>(assert);
        }

        public static IMethodAssert Args<T1, T2, T3, T4>(Func<T1, T2, T3, T4, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2, T3, T4>(assert);
        }

        public static IMethodAssert Args<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2, T3, T4, T5>(assert);
        }

        public static IMethodAssert Args<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2, T3, T4, T5, T6>(assert);
        }

        public static IMethodAssert Args<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2, T3, T4, T5, T6, T7>(assert);
        }

        public static IMethodAssert Args<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2, T3, T4, T5, T6, T7, T8>(assert);
        }

        public static IMethodAssert Args<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2, T3, T4, T5, T6, T7, T8, T9>(assert);
        }

        public static IMethodAssert Args<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool> assert)
        {
            return new MethodApplicabilityChecker<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(assert);
        }

        #endregion

        #region MethodCallback

        public static IMethodCallback Callback(Action callback)
        {
            return new MethodCallback(callback);
        }

        public static IMethodCallback Callback<T>(Action<T> callback)
        {
            return new MethodCallback<T>(callback);
        }

        public static IMethodCallback Callback<T1, T2>(Action<T1, T2> callback)
        {
            return new MethodCallback<T1, T2>(callback);
        }

        public static IMethodCallback Callback<T1, T2, T3>(Action<T1, T2, T3> callback)
        {
            return new MethodCallback<T1, T2, T3>(callback);
        }

        public static IMethodCallback Callback<T1, T2, T3, T4>(Action<T1, T2, T3, T4> callback)
        {
            return new MethodCallback<T1, T2, T3, T4>(callback);
        }

        public static IMethodCallback Callback<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> callback)
        {
            return new MethodCallback<T1, T2, T3, T4, T5>(callback);
        }

        public static IMethodCallback Callback<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> callback)
        {
            return new MethodCallback<T1, T2, T3, T4, T5, T6>(callback);
        }

        public static IMethodCallback Callback<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> callback)
        {
            return new MethodCallback<T1, T2, T3, T4, T5, T6, T7>(callback);
        }

        public static IMethodCallback Callback<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> callback)
        {
            return new MethodCallback<T1, T2, T3, T4, T5, T6, T7, T8>(callback);
        }

        public static IMethodCallback Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> callback)
        {
            return new MethodCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9>(callback);
        }

        public static IMethodCallback Callback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> callback)
        {
            return new MethodCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(callback);
        }

        #endregion
    }
}
