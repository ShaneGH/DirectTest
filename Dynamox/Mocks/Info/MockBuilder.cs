using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Compile;
using Dynamox.Dynamic;

namespace Dynamox.Mocks.Info
{
    /// <summary>
    /// The core of building mocks. Should be cast as a dynamic and used to build mock information
    /// </summary>
    internal class MockBuilder : DynamicBag, IRaiseEvent
    {
        IEnumerable<object> ConstructorArgs;
        private readonly Dictionary<Type, Mock> Concrete = new Dictionary<Type, Mock>();

        public readonly DxSettings TestSettings;

        internal readonly ReservedTermsContainer MockSettings;

        public MockBuilder(IEnumerable<object> constructorArgs = null)
            : this(Dx.Settings, constructorArgs)
        {
        }

        public MockBuilder(DxSettings testSettings, IEnumerable<object> constructorArgs = null)
            : this(new ReservedTerms(), testSettings, constructorArgs)
        {
        }

        public MockBuilder(IReservedTerms mockSettings, DxSettings testSettings, IEnumerable<object> constructorArgs = null)
        {
            ConstructorArgs = constructorArgs ?? Enumerable.Empty<object>();
            MockSettings = new ReservedTermsContainer(mockSettings);
            TestSettings = testSettings;
        }

        private static Assembly CurrentAssembly = typeof(MockBuilder).Assembly;
        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type.IsAssignableFrom(GetType()) && binder.Type.Assembly == CurrentAssembly)
                result = this;
            else
                result = Mock(binder.Type);

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // ensure we don't override a mock
            object existingMock;
            if (TryGetMember(binder.Name, out existingMock) && existingMock is MethodGroup)
                throw new InvalidOperationException("The member \"" + binder.Name + "\" has already been mocked as a function, and cannot be set as a property");    //TODM

            return base.TrySetMember(binder, value);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // you cannot get a mock
            object existingMock;
            if (TryGetMember(binder.Name, out existingMock) && existingMock is MethodGroup)
                throw new InvalidOperationException("The member \"" + binder.Name + "\" has already been mocked as a function, and cannot be retrieved as a property");    //TODM

            if (base.TryGetMember(binder, out result))
                return true;

            SetMember(binder.Name, new MockBuilder(MockSettings.Next(), TestSettings));

            return base.TryGetMember(binder, out result);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (base.TryGetIndex(binder, indexes, out result))
                return true;

            SetIndex(indexes, new MockBuilder(MockSettings.Next(), TestSettings));

            return base.TryGetIndex(binder, indexes, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (binder.Name == MockSettings.Use_Unsafe(s => s.Clear))
            {
                Clear();
                result = this;
            }
            else if (binder.Name == MockSettings.Use_Unsafe(s => s.Constructor))
            {
                result = _Constructor(args);
            }
            else if (binder.Name == MockSettings.Use_Unsafe(s => s.As))
            {
                result = _As(binder, args);
            }
            else
            {
                result = MockMethod(binder.Name, GenericArguments(binder), args);
            }

            return true;
        }

        object _Constructor(object[] args)
        {
            ConstructorArgs = args;
            return this;
        }

        object _As(InvokeMemberBinder binder, object[] args)
        {
            Type convert = null;
            if (args.Length == 1 && args[0] is Type)
            {
                convert = args[0] as Type;
            }
            else
            {
                var typeArgs = GenericArguments(binder);
                if (typeArgs == null || typeArgs.Count != 1)
                    throw new InvalidOperationException("A call to " + MockSettings.Use_Unsafe(s => s.As) + " must have 1 generic type argument or 1 argument for return type.");

                convert = typeArgs[0];
            }

            return Mock(convert);
        }

        static IList<Type> GenericArguments(InvokeMemberBinder binder)
        {
            //TOOD: http://stackoverflow.com/questions/5492373/get-generic-type-of-call-to-method-in-dynamic-object
            var csharpBinder = binder.GetType().GetInterface("Microsoft.CSharp.RuntimeBinder.ICSharpInvokeOrInvokeMemberBinder");
            return csharpBinder.GetProperty("TypeArguments").GetValue(binder, null) as IList<Type> ?? new List<Type>();
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            if (args.Length == 1 && args[0] is IReservedTerms)
            {
                MockSettings.Set(args[0] as IReservedTerms);
                result = this;
            }
            else
            {
                result = MockMethod(string.Empty, Enumerable.Empty<Type>(), args);
            }
            
            return true;
        }

        protected object GetOrMockProperty(string name)
        {
            object result;

            // get
            if (base.TryGetMember(name, out result))
            {
                if (result is MockBuilder)
                    (result as MockBuilder).MockSettings.Set(MockSettings);

                return result;
            }

            // or mock
            SetMember(name, result = new MockBuilder(MockSettings.Next(), TestSettings));
            return result;
        }

        public object Mock(Type mockType)
        {
            lock (Concrete)
            {
                if (!Concrete.ContainsKey(mockType))
                    Concrete.Add(mockType, new Mock(mockType, this, TestSettings, ConstructorArgs));

                return Concrete[mockType].Object;
            }
        }

        protected MethodMockBuilder MockMethod(string name, IEnumerable<Type> genericArgs, object[] args)
        {
            object existingMock;
            if (TryGetMember(name, out existingMock) && !(existingMock is MethodGroup))
                throw new InvalidOperationException("The member \"" + name + "\" has already been set as a parameter, and cannot be mocked now as a function");    //TODM

            var settings = MockSettings.Next();
            var result = new MethodMockBuilder(settings, new MockBuilder(settings, TestSettings), genericArgs, args);
            if (existingMock == null)
            {
                existingMock = new MethodGroup(result);
                SetMember(name, existingMock);
            }
            else
            {
                //TODM: if a method is mocked twice, the second mock will take precedence
                (existingMock as MethodGroup).Insert(0, result);
            }

            return result;
        }

        /// <summary>
        /// Gets a list of messages, each one detailing a method which was marked for being called
        /// </summary>
        public IEnumerable<string> ShouldHaveBeenCalled
        {
            get
            {
                return Values
                    // methods
                    .Where(v => v.Value is MethodGroup)
                    .Select(v => new { name = v.Key, args = (v.Value as MethodGroup).ShouldHaveBeenCalled })
                    .SelectMany(v => v.args.Select(a => "Method: " + v.name + (a.Any() ? "{ " + a  + " }" : string.Empty)))
                    .Concat(Values
                        // properties
                        .Where(v => v.Value is MockBuilder)
                        .Select(v => new { name = v.Key, args = (v.Value as MockBuilder).ShouldHaveBeenCalled })
                        .SelectMany(v => v.args.Select(a => "Method: " + v.name + (a.Any() ? "{ " + a + " }" : string.Empty))));
            }
        }

        #region events

        readonly List<IEventHandler> EventHandlers = new List<IEventHandler>();

        void AddEventHandler(IEventHandler eventHandler)
        {
            //TODO: test args against all existing event handlers
            EventHandlers.Add(eventHandler);
        }

        void RemoveEventHandler(IEventHandler eventHandler)
        {
            EventHandlers.Remove(eventHandler);
        }

        public static MockBuilder operator +(MockBuilder mb, IEventHandler eventHandler)
        {
            mb.AddEventHandler(eventHandler);
            return mb;
        }

        public static MockBuilder operator -(MockBuilder mb, IEventHandler eventHandler)
        {
            mb.RemoveEventHandler(eventHandler);
            return mb;
        }

        public bool RaiseEvent(string eventName, object[] args)
        {
            throw new NotImplementedException();
            //var chainArgs = new EventChainArgs(this, eventName, args);
            //if (EventBubble != null)
            //    EventBubble(chainArgs);

            //EventTunnel(chainArgs);

            //return chainArgs.EventHandlerFound;
        }

        #endregion
    }
}
