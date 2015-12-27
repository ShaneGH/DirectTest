using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
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

        public bool TryInvoke(IEnumerable<Type> genericArguments, IEnumerable<MethodArg> arguments, out object result) 
        {
            foreach (var item in this)
                if (item.TryInvoke(genericArguments, arguments, out result))
                    return true;
            
            result = null;
            return false;
        }

        /// <summary>
        /// Returns a a summary of the args of any method which was strict mocked and not called
        /// </summary>
        public IEnumerable<string> ShouldHaveBeenCalled 
        {
            get
            {
                return this.Where(m => m.MustBeCalled && !m.WasCalled)
                    .Select(m => m.ArgChecker.InputTypes.Any() ? "Args: " + string.Join(", ", m.ArgChecker.InputTypes) : string.Empty)
                    .Union(
                        this.Select(m => m.ReturnValue).OfType<MockBuilder>().SelectMany(b => b.ShouldHaveBeenCalled));
            }
        }
    }

    internal class MethodMockBuilder : DynamicObject
    {
        public readonly IMethodAssert ArgChecker;
        public object ReturnValue { get; private set; }
        public readonly List<Tuple<int, object>> OutParamValues = new List<Tuple<int,object>>();
        public bool MustBeCalled { get; private set; }
        public bool WasCalled { get; private set; }

        readonly IEnumerable<Type> GenericArguments;
        readonly ReadOnlyDictionary<string, Func<object[], bool>> SpecialActions;
        readonly MockBuilder NextPiece;

        readonly List<IMethodCallback> Actions = new List<IMethodCallback>();

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
                throw new InvalidOperationException("Arg checker must be first and only arg");  //TODE
            else
                ArgChecker = new EqualityMethodApplicabilityChecker(args);

            ArgChecker.OutParamValues = OutParamValues;

            GenericArguments = Array.AsReadOnly((genericArgs ?? Enumerable.Empty<Type>()).ToArray());
            ReturnValue = nextPiece;
            NextPiece = nextPiece;
            MustBeCalled = false;
            WasCalled = false;

            SpecialActions = new ReadOnlyDictionary<string, Func<object[], bool>>(new Dictionary<string, Func<object[], bool>> 
            {
                { settings.Out, Out },
                { settings.Returns, Returns },
                { settings.Ensure, Ensure },
                { settings.Do, Do }
            });
        }

        bool Out(object[] args)
        {
            if (args.Length != 2 || !(args[0] is int))
                throw new InvalidOperationException("The arguments for Out are [int, object].");   //TODE

            int index = (int)args[0];
            if (OutParamValues.Any(o => o.Item1 == index))
                throw new InvalidOperationException("There is already an out value defined for this index [" + index + "].");   //TODE

            OutParamValues.Add(new Tuple<int, object>(index, args[1]));

            return true;
        }

        bool Returns(object[] args)
        {
            if (args.Length != 1)
                throw new InvalidOperationException("You must specify a single argument to return.");   //TODE

            ReturnValue = args[0];

            return true;
        }

        bool Ensure(object[] args)
        {
            if (args != null && args.Any())
                throw new InvalidOperationException("You cannot pass any argments into ensure");   //TODE

            MustBeCalled = true;
            Actions.Add(new MethodCallback(() => WasCalled = true));

            return true;
        }

        bool Do(object[] args)
        {
            if (args.Length != 1)
                throw new InvalidOperationException("You must specify the action to do."); //TODE

            if (!(args[0] is IMethodCallback))
                throw new InvalidOperationException("The first arg must be an IMethodCallback."); //TODE

            Actions.Add(args[0] as IMethodCallback);

            return true;
        }

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

            if (SpecialActions[binder.Name](args))
                result = this;
            else
                result = null;

            return true;
        }

        public bool TryInvoke(IEnumerable<MethodArg> arguments, out object result)
        {
            return TryInvoke(Enumerable.Empty<Type>(), arguments, out result);
        }

        public bool RepresentsMethod(MethodInfo method)
        {
            //TODO: generic constraints???
            var methodGenerics = method.GetGenericArguments();
            if (GenericArguments.Count() != methodGenerics.Length)
            {
                // TODO: reason
                return false;
            }

            for (var i = 0; i < methodGenerics.Length; i++)
            {
                // is constructed generic method
                if (!method.ContainsGenericParameters)
                {
                    if (methodGenerics[i] != GenericArguments.ElementAt(i))
                        return false;
                }
                else // is generic method
                {
                    var constraints = methodGenerics[i].GetGenericParameterConstraints();
                }
            }

            //TODO: out params
            return ArgChecker.CanMockMethod(method);
        }
        
        public bool TryInvoke(IEnumerable<Type> genericArguments, IEnumerable<MethodArg> arguments, out object result)
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
                foreach (var _out in OutParamValues.Where(p => p.Item1 < arguments.Count()))
                    arguments.ElementAt(_out.Item1).Arg = _out.Item2;
                    

                foreach (var after in Actions)
                    if (!after.Do(arguments.Select(a => a.Arg)))
                        throw new InvalidOperationException("Bad type args");

                return true;
            }

            result = null;
            return false;
        }
    }
}
