using System;
using System.Dynamic;
using System.Linq;

namespace DirectTests
{
    public class TestArranger : TestBag
    {
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (base.TryGetMember(binder, out result))
                return true;

            SetMember(binder.Name, new MockBuilder());   //TODO

            return base.TryGetMember(binder, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (args.Length != 1)
                throw new InvalidOperationException("There can only be one argument to this method: the mock settings.");

            if (!base.TryGetMember(binder.Name, out result))
            {
                SetMember(binder.Name, result = new MockBuilder(args[0] is MockSettings ? args[0] as MockSettings : args[0]));
                return true;
            }

            if (!(result is MockBuilder))
                throw new InvalidOperationException("The member \"" + binder.Name + "\" has already been set as a property, and cannot be mocked");    //TODM

            (result as MockBuilder).Settings = args[0] is MockSettings ? args[0] as MockSettings : new MockSettings(args[0]);

            return true;
        }

        public void SetAllSettingsToDefault() 
        {
            foreach (var builder in Values.Values.OfType<MockBuilder>())
                builder.Settings = null;
        }
    }
}
