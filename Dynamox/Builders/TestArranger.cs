using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Dynamox.Dynamic;
using Dynamox.Mocks;

namespace Dynamox.Builders
{
    public class TestArranger : DynamicBag
    {
        public readonly DxSettings Settings;

        public TestArranger(DxSettings settings)
        {
            Settings = settings;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!base.TryGetMember(binder, out result))
                SetMember(binder.Name, result = new MockBuilder(Settings));

            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (args.Length != 1)
                throw new InvalidOperationException("There can only be one argument to this method: the mock settings.");

            if (!base.TryGetMember(binder.Name, out result))
            {
                SetMember(binder.Name, result = new MockBuilder(args[0] is MockSettings ? args[0] as MockSettings : args[0], Settings));
                return true;
            }

            if (!(result is MockBuilder))
                throw new InvalidOperationException("The member \"" + binder.Name + "\" has already been set as a property, and cannot be mocked");    //TODM

            (result as MockBuilder).MockSettings = args[0] is MockSettings ? args[0] as MockSettings : new MockSettings(args[0]);

            return true;
        }

        public void SetAllSettingsToDefault() 
        {
            foreach (var builder in Values.Values.OfType<MockBuilder>())
                builder.MockSettings = null;
        }

        public IEnumerable<string> ShouldHaveBeenCalled
        {
            get
            {
                return Values
                    .Where(v => v.Value is MockBuilder)
                    .Select(v => new { name = v.Key, args = (v.Value as MockBuilder).ShouldHaveBeenCalled })
                    .SelectMany(v => v.args.Select(a => "testBag." + v.name + ": { " + a + " }"));
            }
        }
    }
}
