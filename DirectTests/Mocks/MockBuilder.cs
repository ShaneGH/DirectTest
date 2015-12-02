using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DirectTests.Dynamic;

namespace DirectTests.Mocks
{
    /// <summary>
    /// This class is not thread safe.
    /// It is intended to be used multiple times, however the Settings property will be changed at the beginning of each operation
    /// </summary>
    internal class MockBuilder : DynamicBag
    {
        private readonly Dictionary<Type, Mock> Concrete = new Dictionary<Type, Mock>();

        private MockSettings _Settings;
        internal MockSettings Settings 
        {
            get 
            {
                return _Settings ?? (_Settings = new MockSettings());
            }
            set 
            {
                _Settings = value;
            }
        }

        public MockBuilder()
            : this(new MockSettings())
        {
        }

        public MockBuilder(MockSettings settings)
        {
            Settings = settings;
        }

        public MockBuilder(object settings)
            : this(new MockSettings(settings))
        {
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

            SetMember(binder.Name, new MockBuilder(Settings));

            return base.TryGetMember(binder, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (binder.Name == Settings.Clear)
            {
                Clear();
                result = this;
                return true;
            }

            if (binder.Name == Settings.As)
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
                        throw new InvalidOperationException("A call to " + Settings.As + " must have 1 generic type argument or 1 argument for return type.");

                    convert = typeArgs[0];
                }

                result = Mock(convert);
            }
            else
            {
                result = MockMethod(binder.Name, GenericArguments(binder), args);
            }

            return true;
        }

        static IList<Type> GenericArguments(InvokeMemberBinder binder)
        {
            //TOOD: http://stackoverflow.com/questions/5492373/get-generic-type-of-call-to-method-in-dynamic-object
            var csharpBinder = binder.GetType().GetInterface("Microsoft.CSharp.RuntimeBinder.ICSharpInvokeOrInvokeMemberBinder");
            return csharpBinder.GetProperty("TypeArguments").GetValue(binder, null) as IList<Type> ?? new List<Type>();
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            if (args.Length == 1 && args[0] is MockSettings)
            {
                Settings = args[0] as MockSettings;
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
            if (base.TryGetMember(name, out result))
            {
                if (result is MockBuilder)
                    (result as MockBuilder).Settings = Settings;

                return result;
            }

            SetMember(name, result = new MockBuilder(Settings));
            return result;
        }

        public object Mock(Type mockType)
        {
            lock (Concrete)
            {
                if (!Concrete.ContainsKey(mockType))
                    Concrete.Add(mockType, new Mock(mockType, this));

                return Concrete[mockType].Object;
            }
        }

        protected MethodMockBuilder MockMethod(string name, IEnumerable<Type> genericArgs, object[] args)
        {
            object existingMock;
            if (TryGetMember(name, out existingMock) && !(existingMock is MethodGroup))
                throw new InvalidOperationException("The member \"" + name + "\" has already been set as a parameter, and cannot be mocked now as a function");    //TODM

            var result = new MethodMockBuilder(Settings, new MockBuilder(Settings), genericArgs, args);
            if (existingMock == null)
            {
                existingMock = new MethodGroup(result);
                SetMember(name, existingMock);
            }
            else
            {
                (existingMock as MethodGroup).Add(result);
            }

            return result;
        }

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
    }
}
