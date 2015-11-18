using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests
{
    /// <summary>
    /// This class is not thread safe.
    /// It is intended to be used multiple times, however the Settings property will be changed at the beginning of each operation
    /// </summary>
    internal class MockBuilder : TestBag
    {
        private MockSettings _Settings;
        internal MockSettings Settings 
        {
            get 
            {
                return _Settings ?? (_Settings = new MockSettings());
            }
            set 
            {
                _Settings = Settings;
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

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            // finalize mock
            return base.TryConvert(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // ensure we don't override a mock
            object existingMock;
            if (TryGetMember(binder.Name, out existingMock) && existingMock is MethodMockBuilderCollection)
                throw new InvalidOperationException("The member \"" + binder.Name + "\" has already been mocked as a function, and cannot be set as a property");    //TODM

            return base.TrySetMember(binder, value);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // you cannot get a mock
            object existingMock;
            if (TryGetMember(binder.Name, out existingMock) && existingMock is MethodMockBuilderCollection)
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

            result = MockMethod(binder.Name, args);
            return true;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = MockMethod(string.Empty, args);
            return true;
        }

        protected object GetOrMockProperty(string name)
        {
            object result;
            if (base.TryGetMember(name, out result))
                return result;

            SetMember(name, result = new MockBuilder(Settings));
            return result;
        }

        protected MethodMockBuilder MockMethod(string name, object[] args)
        {
            object existingMock;
            if (TryGetMember(name, out existingMock) && !(existingMock is MethodMockBuilderCollection))
                throw new InvalidOperationException("The member \"" + name + "\" has already been set as a parameter, and cannot be mocked now as a function");    //TODM

            var result = new MethodMockBuilder(Settings, new MockBuilder(Settings));
            if (existingMock == null)
            {
                existingMock = new MethodMockBuilderCollection(result);
                SetMember(name, existingMock);
            }
            else
            {
                (existingMock as MethodMockBuilderCollection).Add(result);
            }

            return result;
        }

        private class MethodMockBuilderCollection : Collection<MethodMockBuilder>
        {
            public MethodMockBuilderCollection()
                : base()
            {
            }

            public MethodMockBuilderCollection(MethodMockBuilder first)
                : this()
            {
                Add(first);
            }
        }
    }
}
