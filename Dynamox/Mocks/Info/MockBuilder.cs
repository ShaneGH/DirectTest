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
    internal class MockBuilder : DynamicBag, IRaiseEvent, IEventParasite, IEnsure
    {
        IEnumerable<object> ConstructorArgs;
        private readonly Dictionary<Type, Mock> Concrete = new Dictionary<Type, Mock>();

        private readonly HashSet<string> EnsuredMembers = new HashSet<string>();
        private readonly HashSet<string> AccessedMembers = new HashSet<string>();

        private readonly List<IReadOnlyCollection<object>> EnsuredIndexedProperties = new List<IReadOnlyCollection<object>>();
        private readonly List<IReadOnlyCollection<object>> AccessedIndexedProperties = new List<IReadOnlyCollection<object>>();

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

        public override void SetMember(string name, object value)
        {
            // ensure we don't override a method mock
            if (Values.ContainsKey(name) && Values[name] is MethodGroup)
                throw new InvalidOperationException("The member \"" + name + "\" has already been mocked as a method, and cannot be set as a property");    //TODM
            
            if (value is IMethod)
            {
                SetMethod(name, value as IMethod);
                return;
            }

            if (value is IEnsuredProperty)
            {
                var temp = value as IEnsuredProperty;
                value = temp.PropertyValue;
                if (temp.IsEnsured && !EnsuredMembers.Contains(name))
                    EnsuredMembers.Add(name);
            }

            base.SetMember(name, value);
        }

        void SetMethod(string name, IMethod value)
        {
            var method = MockMethod(name, Enumerable.Empty<Type>(), value.ArgTypes.Select(a => Dx.Any(a)));

            if (value.Ensured)
                method.Ensure();

            method.Actions.Add(new DelegateMethodCallback(
                a => method.ReturnValue = value.Invoke(a), value.ArgTypes));
        }

        public override bool TryGetMember(string name, out object result)
        {
            // you cannot get a method mock
            if (base.TryGetMember(name, out result))
            {
                if (result is MethodGroup)
                    throw new InvalidOperationException("The member \"" + name + 
                        "\" has already been mocked as a method, and cannot be retrieved as a property");    //TODM
                else
                    return true;
            }

            SetMember(name, new MockBuilder(MockSettings.Next(), TestSettings));
            return base.TryGetMember(name, out result);
        }

        public override void SetIndex(IEnumerable<object> key, object value)
        {
            if (value is IEnsuredProperty)
            {
                var temp = value as IEnsuredProperty;
                value = temp.PropertyValue;
                if (temp.IsEnsured)
                    EnsuredIndexedProperties.Add(key.ToList().AsReadOnly());
            }

            base.SetIndex(key, value);
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
            if (binder.Name == MockSettings.Use_Unsafe(s => s.DxClear))
            {
                Clear();
                result = this;
            }
            else if (binder.Name == MockSettings.Use_Unsafe(s => s.DxConstructor))
            {
                result = _Constructor(args);
            }
            else if (binder.Name == MockSettings.Use_Unsafe(s => s.DxAs))
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
                    throw new InvalidOperationException("A call to " + MockSettings.Use_Unsafe(s => s.DxAs) + " must have 1 generic type argument or 1 argument for return type.");

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

        /// <summary>
        /// Tell the mock builder that the following property has been retrieved
        /// </summary>
        /// <param name="memberName"></param>
        public void AccessedProperty(string memberName)
        {
            if (!AccessedMembers.Contains(memberName))
                AccessedMembers.Add(memberName);
        }

        /// <summary>
        /// Tell the mock builder that the following indexed property has been retrieved
        /// </summary>
        /// <param name="memberName"></param>
        public void AccessedIndexedProperty(IEnumerable<object> keys)
        {
            AccessedIndexedProperties.Add(keys.ToList().AsReadOnly());
        }

        protected internal MethodMockBuilder MockMethod(string name, IEnumerable<Type> genericArgs, IEnumerable<object> args)
        {
            MethodGroup existingMock = null;
            if (Values.ContainsKey(name))
            {
                existingMock = Values[name] as MethodGroup;
                if (existingMock == null)
                    throw new InvalidMockException("The member \"" + name + "\" has already been set as a parameter and cannot be mocked now as a method");    //TODM
            }

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
                existingMock.Insert(0, result);
            }

            return result;
        }

        /// <summary>
        /// Gets a list of messages, each one detailing a method which was marked to be called
        /// </summary>
        public IEnumerable<string> ShouldHaveBeenCalled
        {
            get
            {
                //TODO, messages need some work

                // methods
                var methods = Values
                    .Where(v => v.Value is MethodGroup)
                    .Select(v => new { name = v.Key, args = (v.Value as MethodGroup).ShouldHaveBeenCalled })
                    .SelectMany(v => v.args.Select(a => v.name + a));

                // nested properties
                var nestedProperties = Values
                    .Where(v => v.Value is MockBuilder)
                    .Select(v => new { name = v.Key, args = (v.Value as MockBuilder).ShouldHaveBeenCalled })
                    .SelectMany(v => v.args.Select(a => v.name + (a.Any() ? "." + a : string.Empty)));

                // propeties
                var properties = Values
                    .Where(v => !(v.Value is MockBuilder) && !(v.Value is MethodGroup))
                    .Where(v => EnsuredMembers.Contains(v.Key))
                    .Where(v => !AccessedMembers.Contains(v.Key))
                    .Select(v => v.Key);

                // nested indexes
                var nestedIndexes = IndexedValues
                    .Where(v => v.Value is MockBuilder)
                    .Select(v => new { keys = v.Keys, args = (v.Value as MockBuilder).ShouldHaveBeenCalled })
                    .SelectMany(v => v.args.Select(a => "[" + string.Join(", ", v.keys) + "]" + (a.Any() ? "." + a : string.Empty)));
                
                // indexes
                var indexes = IndexedValues
                    .Where(v => !(v.Value is MockBuilder))
                    .Where(v => EnsuredIndexedProperties.Any(i => v.CompareKeys(i)))
                    .Where(v => !AccessedIndexedProperties.Any(i => v.CompareKeys(i)))
                    .Select(v => "[" + string.Join(", ", v.Keys) + "]");

                return methods
                    .Concat(nestedProperties)
                    .Concat(properties)
                    .Concat(nestedIndexes)
                    .Concat(indexes);
            }
        }

        #region events

        public bool IsEvent
        {
            get
            {
                return _EventHandlers.Any();
            }
        }

        public IEnumerable<IEventHandler> EventHandlers
        {
            get
            {
                return _EventHandlers.ToArray();
            }
        }

        readonly List<IEventHandler> _EventHandlers = new List<IEventHandler>();

        void AddEventHandler(IEventHandler eventHandler)
        {
            //TODO: test args against all existing event handlers
            _EventHandlers.Add(eventHandler);
        }

        void RemoveEventHandler(IEventHandler eventHandler)
        {
            _EventHandlers.Remove(eventHandler);
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

        public event EventShareHandler RaiseEventCalled;

        public void EventRaised(EventShareEventArgs args)
        {
            // this is an event property
            foreach (var handler in _EventHandlers.Where(e => e.CanBeInvoked(args.EventArgs)))
            {
                args.EventHandlerFound = true;
                handler.Invoke(args.EventArgs);
            }

            // - or -

            // this has an event property
            if (Values.ContainsKey(args.EventName) && Values[args.EventName] is MockBuilder)
                (Values[args.EventName] as MockBuilder).EventRaised(args);
        }

        public bool RaiseEvent(string eventName, object[] args)
        {
            if (RaiseEventCalled == null)
                return false;

            var eventArgs = new EventShareEventArgs(eventName, args);
            RaiseEventCalled(eventArgs);
            return eventArgs.EventHandlerFound;
        }

        #endregion
    }
}
