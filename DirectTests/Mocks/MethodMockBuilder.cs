using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests.Mocks
{
    internal class MethodMockBuilder : DynamicObject
    {
        public readonly IEnumerable<object> Args;
        public object ReturnValue { get; private set; }
        readonly ReadOnlyDictionary<string, Action<object[]>> SpecialActions;
        readonly MockBuilder NextPiece;

        public MethodMockBuilder(MockSettings settings, MockBuilder nextPiece, IEnumerable<object> args)
        {
            NextPiece = nextPiece;

            Args = args.ToArray();

            SpecialActions = new ReadOnlyDictionary<string, Action<object[]>>(new Dictionary<string, Action<object[]>> 
            {
                //TODO, some of these will stop all other functions
                { settings.Return, Return },
                { settings.Ensure, Ensure },
                { settings.Clear, Clear },
                { settings.Do, Do }
            });
        }

        void Return(object[] args)
        {
            if (args.Length != 1)
                throw new InvalidOperationException("You must specify a single argument to return.");

            ReturnValue = args[0];
        }

        void Ensure(object[] args) { }
        void Clear(object[] args) { }
        void Do(object[] args) { }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return NextPiece.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return NextPiece.TrySetMember(binder, value);
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            return NextPiece.TryInvoke(binder, args, out result);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (!SpecialActions.ContainsKey(binder.Name))
                return NextPiece.TryInvokeMember(binder, args, out result);

            SpecialActions[binder.Name](args);
            result = this;
            return true;
        }
    }
}
