using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Builders;
using Dynamox.Compile;
using Dynamox.Mocks;
using Dynamox.Mocks.Info;
using Dynamox.StronglyTyped;

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

        public static AnyValue<T> AnyT<T>()
        {
            return MethodApplicabilityChecker.AnyT<T>();
        }

        public static AnyValue AnyT(Type t)
        {
            return MethodApplicabilityChecker.AnyT(t);
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

        /// <summary>
        /// Create a weakly typed mock
        /// </summary>
        /// <param name="constructorArgs">The constructor args for the mock</param>
        /// <returns>A weakly typed mock object</returns>
        public static dynamic Mock(IEnumerable<object> constructorArgs = null)
        {
            return new MockBuilder(constructorArgs);
        }

        /// <summary>
        /// Create a weakly typed mock
        /// </summary>
        /// <param name="settings">The mock settings</param>
        /// <param name="constructorArgs">The constructor args for the mock</param>
        /// <returns>A weakly typed mock object</returns>
        public static dynamic Mock(DxSettings settings, IEnumerable<object> constructorArgs = null)
        {
            return new MockBuilder(settings, constructorArgs);
        }

        /// <summary>
        /// Create a weakly typed mock
        /// </summary>
        /// <param name="settings">The mock settings</param>
        /// <param name="reservedTerms">The mock reserved terms</param>
        /// <param name="constructorArgs">The constructor args for the mock</param>
        /// <returns>A weakly typed mock object</returns>
        public static dynamic Mock(DxSettings settings, ReservedTerms reservedTerms, IEnumerable<object> constructorArgs = null)
        {
            return new MockBuilder(reservedTerms, settings, constructorArgs);
        }

        /// <summary>
        /// Create a weakly typed mock
        /// </summary>
        /// <param name="settings">The mock settings</param>
        /// <param name="reservedTerms">The mock reserved terms</param>
        /// <param name="constructorArgs">The constructor args for the mock</param>
        /// <returns>A weakly typed mock object</returns>
        public static dynamic Mock(DxSettings settings, object reservedTerms, IEnumerable<object> constructorArgs = null)
        {
            return new MockBuilder(new ReservedTerms(reservedTerms), settings, constructorArgs);
        }

        /// <summary>
        /// Create a strongly typed mock
        /// </summary>
        /// <param name="constructorArgs">The constructor args for the mock</param>
        /// <returns>A weakly typed mock object</returns>
        public static MockBuilder<T> Mock<T>(IEnumerable<object> constructorArgs = null)
        {
            return new MockBuilder<T>(constructorArgs);
        }

        /// <summary>
        /// Create a strongly typed mock
        /// </summary>
        /// <param name="settings">The mock settings</param>
        /// <param name="constructorArgs">The constructor args for the mock</param>
        /// <returns>A weakly typed mock object</returns>
        public static MockBuilder<T> Mock<T>(DxSettings settings, IEnumerable<object> constructorArgs = null)
        {
            return new MockBuilder<T>(settings, constructorArgs);
        }

        /// <summary>
        /// Create a strongly typed mock
        /// </summary>
        /// <param name="settings">The mock settings</param>
        /// <param name="constructorArgs">The constructor args for the mock</param>
        /// <param name="builder">The mock logic. 
        /// The first paramater is a strongly typed mock builder. 
        /// The second paramater is a weakly typed mock builder. 
        /// You can use both interchangably to add mocked logic and parameters</param>
        /// <returns>A mocked object</returns>
        public static T Mock<T>(DxSettings settings, IEnumerable<object> constructorArgs, Action<MockBuilder<T>, dynamic> builder)
        {
            var mock = Mock<T>(settings, constructorArgs);
            builder(mock, mock._mock);
            return mock.DxAs();
        }

        /// <summary>
        /// Create a strongly typed mock
        /// </summary>
        /// <param name="constructorArgs">The constructor args for the mock</param>
        /// <param name="builder">The mock logic. 
        /// The first paramater is a strongly typed mock builder. 
        /// The second paramater is a weakly typed mock builder. 
        /// You can use both interchangably to add mocked logic and parameters</param>
        /// <returns>A mocked object</returns>
        public static T Mock<T>(IEnumerable<object> constructorArgs, Action<MockBuilder<T>, dynamic> builder)
        {
            return Mock(DxSettings.GlobalSettings, constructorArgs, builder);
        }

        /// <summary>
        /// Create a strongly typed mock
        /// </summary>
        /// <param name="builder">The mock logic. 
        /// The first paramater is a strongly typed mock builder. 
        /// The second paramater is a weakly typed mock builder. 
        /// You can use both interchangably to add mocked logic and parameters</param>
        /// <returns>A mocked object</returns>
        public static T Mock<T>(Action<MockBuilder<T>, dynamic> builder)
        {
            return Mock(DxSettings.GlobalSettings, null, builder);
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
        /// <param name="mocks">The mocked objects or mock builders</param>
        public static void Ensure(params object[] mocks)
        {
            if (mocks.Any(m => !(m is IEnsure)))
                throw new InvalidMockException("You can only call this method on mocks created with Dx.Mock(). " + 
                    "If you are calling on a mocked object, the mocked object must be a referece type (not a struct) and not sealed.");

            var errors = mocks
                .Cast<IEnsure>()
                .SelectMany(b => b.ShouldHaveBeenCalled);

            if (!errors.Any())
                return;

            throw new MockedMethodNotCalledException(errors);
        }

        #region Events

        public static bool RaiseEvent(object target, string eventName, params object[] eventArgs)
        {
            var raiseEvent = target as IRaiseEvent;
            if (raiseEvent == null)
                throw new InvalidMockException("Object " + target + " is not a Dynamox object and it's events cannot be raised by this method.");

            return raiseEvent.RaiseEvent(eventName, eventArgs);
        }

        public static IEventHandler EventHandler(Action eventHandler)
        {
            return new DynamoxEventHandler(eventHandler);
        }

        public static IEventHandler EventHandler<TArg1>(
            Action<TArg1> eventHandler)
        {
            return new DynamoxEventHandler<TArg1>(eventHandler);
        }

        public static IEventHandler EventHandler<TArg1, TArg2>(
            Action<TArg1, TArg2> eventHandler)
        {
            return new DynamoxEventHandler<TArg1, TArg2>(eventHandler);
        }

        public static IEventHandler EventHandler<TArg1, TArg2, TArg3>(
            Action<TArg1, TArg2, TArg3> eventHandler)
        {
            return new DynamoxEventHandler<TArg1, TArg2, TArg3>(eventHandler);
        }

        public static IEventHandler EventHandler<TArg1, TArg2, TArg3, TArg4>(
            Action<TArg1, TArg2, TArg3, TArg4> eventHandler)
        {
            return new DynamoxEventHandler<TArg1, TArg2, TArg3, TArg4>(eventHandler);
        }

        public static IEventHandler EventHandler<TArg1, TArg2, TArg3, TArg4, TArg5>(
            Action<TArg1, TArg2, TArg3, TArg4, TArg5> eventHandler)
        {
            return new DynamoxEventHandler<TArg1, TArg2, TArg3, TArg4, TArg5>(eventHandler);
        }

        public static IEventHandler EventHandler<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(
            Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> eventHandler)
        {
            return new DynamoxEventHandler<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(eventHandler);
        }

        public static IEventHandler EventHandler<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(
            Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> eventHandler)
        {
            return new DynamoxEventHandler<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(eventHandler);
        }

        public static IEventHandler EventHandler<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(
            Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> eventHandler)
        {
            return new DynamoxEventHandler<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(eventHandler);
        }

        public static IEventHandler EventHandler<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>(
            Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9> eventHandler)
        {
            return new DynamoxEventHandler<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>(eventHandler);
        }

        public static IEventHandler EventHandler<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10>(
            Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10> eventHandler)
        {
            return new DynamoxEventHandler<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10>(eventHandler);
        }

        #endregion

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
