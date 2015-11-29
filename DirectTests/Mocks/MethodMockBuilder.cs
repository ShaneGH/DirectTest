using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests.Mocks
{
    internal class MethodGroup : Collection<MethodMockBuilder>
    {
        public MethodGroup()
            : base()
        {
        }

        public MethodGroup(MethodMockBuilder first)
            : this()
        {
            Add(first);
        }

        public bool TryInvoke(IEnumerable<Type> genericArguments, IEnumerable<object> arguments, out object result) 
        {
            foreach (var item in this)
                if (item.TryInvoke(genericArguments, arguments, out result))
                    return true;
            
            result = null;
            return false;
        }             
    }

    internal class MethodMockBuilder : DynamicObject
    {
        public readonly IMethodAssert ArgChecker;
        public object ReturnValue { get; private set; }

        readonly IEnumerable<Type> GenericArguments;
        readonly ReadOnlyDictionary<string, Action<object[]>> SpecialActions;
        readonly MockBuilder NextPiece;

        public MethodMockBuilder(MockBuilder nextPiece, IEnumerable<object> args)
            : this(nextPiece, Enumerable.Empty<Type>(), args)
        {
        }

        public MethodMockBuilder(MockBuilder nextPiece, IEnumerable<Type> genericArgs, IEnumerable<object> args)
            : this(new MockSettings(), nextPiece, genericArgs, args)
        {
        }

        public MethodMockBuilder(MockSettings settings, MockBuilder nextPiece, IEnumerable<object> args)
            : this(settings, nextPiece, Enumerable.Empty<Type>(), args)
        {
        }

        public MethodMockBuilder(MockSettings settings, MockBuilder nextPiece, IEnumerable<Type> genericArgs, IEnumerable<object> args)
        {
            if (args.Count() == 1 && args.First() is IMethodAssert)
                ArgChecker = args.First() as IMethodAssert;
            else if (args.Any(a => a is IMethodAssert))
                throw new InvalidOperationException("Arg checker must be first and only arg");  //TODO
            else
                ArgChecker = new EqualityMethodApplicabilityChecker(args);

            GenericArguments = Array.AsReadOnly((genericArgs ?? Enumerable.Empty<Type>()).ToArray());
            ReturnValue = nextPiece;
            NextPiece = nextPiece;

            SpecialActions = new ReadOnlyDictionary<string, Action<object[]>>(new Dictionary<string, Action<object[]>> 
            {
                //TODO, some of these will stop all other functions
                { settings.Returns, Return },
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

        public bool TryInvoke(IEnumerable<object> arguments, out object result)
        {
            return TryInvoke(Enumerable.Empty<Type>(), arguments, out result);
        }


        public bool TryInvoke(IEnumerable<Type> genericArguments, IEnumerable<object> arguments, out object result)
        {
            var gen1 = GenericArguments.ToArray();
            var gen2 = genericArguments.ToArray();
            if (gen1.Length != gen2.Length)
            {
                result = null;
                return false;
            }

            for (var i = 0; i < gen1.Length; i++)
            {
                if (gen1[i] != gen2[i])
                {
                    result = null;
                    return false;
                }
            }

            if (ArgChecker.TestArgs(arguments))
            {
                result = ReturnValue;
                return true;
            }

            result = null;
            return false;
        }
    }
}
