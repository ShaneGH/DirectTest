using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks.Info
{
    /// <summary>
    /// Builds method mock functionality. Created and returned by MockBuilder
    /// </summary>
    internal class MethodMockBuilder : DynamicObject
    {
        internal readonly ReservedTermsContainer ReservedTerms;
        public readonly IMethodAssert ArgChecker;
        public object ReturnValue { get; internal set; }
        public readonly List<OutArg> OutParamValues = new List<OutArg>();
        public bool MustBeCalled { get; private set; }
        public bool WasCalled { get; private set; }

        readonly IEnumerable<Type> GenericArguments;
        readonly MockBuilder NextPiece;

        public readonly List<IMethodCallback> Actions = new List<IMethodCallback>();

        public MethodMockBuilder(MockBuilder nextPiece, IEnumerable<object> args)
            : this(nextPiece, Enumerable.Empty<Type>(), args)
        {
        }

        public MethodMockBuilder(MockBuilder nextPiece, IEnumerable<Type> genericArgs, IEnumerable<object> args)
            : this(new ReservedTerms(), nextPiece, genericArgs, args)
        {
        }

        public MethodMockBuilder(IReservedTerms settings, MockBuilder nextPiece, IEnumerable<object> args)
            : this(settings, nextPiece, Enumerable.Empty<Type>(), args)
        {
        }

        public MethodMockBuilder(IReservedTerms terms, MockBuilder nextPiece, IEnumerable<Type> genericArgs, IEnumerable<object> args)
        {
            if (args.Count() == 1 && args.First() is IMethodAssert)
                ArgChecker = args.First() as IMethodAssert;
            else if (args.Any(a => a is IMethodAssert))
                throw new InvalidMockException("An IMethodAssert from Dx.Args<>(...) must be the first and only argument");
            else
                ArgChecker = new EqualityMethodApplicabilityChecker(args);

            ReservedTerms = new ReservedTermsContainer(terms);

            ArgChecker.OutParamValues = OutParamValues;

            GenericArguments = Array.AsReadOnly((genericArgs ?? Enumerable.Empty<Type>()).ToArray());
            ReturnValue = nextPiece;
            NextPiece = nextPiece;
            MustBeCalled = false;
            WasCalled = false;
        }

        bool Out(object[] args)
        {
            if (args.Length != 2 || (!(args[0] is int) && !(args[0] is string)))
                throw new InvalidMockException("The arguments for Out are [int, object] or [string, object].");

            if (args[0] is int)
            {
                int index = (int)args[0];
                if (OutParamValues.Any(o => o.Index == index))
                    throw new InvalidMockException("There is already an out value defined for this index [" + index + "].");

                OutParamValues.Add(new OutArg(index, args[1]));
            }
            else if (args[0] is string)
            {
                string name = (string)args[0];
                if (OutParamValues.Any(o => o.Name == name))
                    throw new InvalidMockException("There is already an out value defined for this name \"" + name + "\".");

                OutParamValues.Add(new OutArg(name, args[1]));
            }


            return true;
        }

        bool Returns(object[] args)
        {
            if (args.Length != 1)
                throw new InvalidMockException("You must specify a single argument to return.");

            ReturnValue = args[0];

            return true;
        }

        public bool Ensure()
        {
            MustBeCalled = true;
            Actions.Add(new MethodCallback(() => WasCalled = true));

            return true;
        }

        bool Do(object[] args)
        {
            if (args.Length != 1)
                throw new InvalidMockException("You must specify a single action to do.");

            if (!(args[0] is IMethodCallback))
                throw new InvalidMockException("The first arg must be an IMethodCallback from Dx.Callback<>(...).");

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
            if (binder.Name == ReservedTerms.Use_Unsafe(a => a.DxOut))
            {
                result = Out(args) ? this : null;
                return true;
            }

            if (binder.Name == ReservedTerms.Use_Unsafe(a => a.DxReturns))
            {
                result = Returns(args) ? this : null;
                return true;
            }

            if (binder.Name == ReservedTerms.Use_Unsafe(a => a.DxDo))
            {
                result = Do(args) ? this : null;
                return true;
            }

            if (binder.Name == ReservedTerms.Use_Unsafe(a => a.DxEnsure))
            {
                if (args != null && args.Any())
                    throw new InvalidMockException("You cannot pass any argments into ensure");

                result = Ensure() ? this : null;
                return true;
            }
            
            return NextPiece.TryInvokeMember(binder, args, out result);
        }

        public bool TryInvoke(IEnumerable<MethodArg> arguments, out object result)
        {
            return TryInvoke(Enumerable.Empty<Type>(), arguments, out result);
        }

        //TODO: should be in an interface
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

            return ArgChecker.CanMockMethod(method);
        }

        //TODO: should be in an interface
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
                foreach (var _out in OutParamValues.Where(p => p.Index >= 0 && p.Index < arguments.Count()))
                    arguments.ElementAt(_out.Index).Arg = _out.Value;
                foreach (var _out in OutParamValues.Where(p => p.Name != null))
                    arguments.Where(a => a.ArgName == _out.Name).ForEach(a => a.Arg = _out.Value);

                foreach (var after in Actions)
                    if (!after.Do(arguments.Select(a => a.Arg)))
                        throw new InvalidOperationException("Bad type args");

                result = ReturnValue;
                return true;
            }

            result = null;
            return false;
        }
    }
}
