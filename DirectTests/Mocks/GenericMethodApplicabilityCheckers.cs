using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests.Mocks
{
    //TODO: MORE!!!!!

    internal class MethodApplicabilityChecker<T> : MethodApplicabilityChecker
    {
        readonly Func<T, bool> ArgsAreValid;

        public override IEnumerable<Type> InputTypes
        {
            get
            {
                yield return typeof(T);
            }
        }

        public MethodApplicabilityChecker(Func<T, bool> argsAreValid)
        {
            ArgsAreValid = argsAreValid;
        }

        protected override bool _TestArgs(IEnumerable<object> args)
        {
            var a = args.ToArray();
            return ArgsAreValid((T)a[0]);
        }
    }

    internal class MethodApplicabilityChecker<T1, T2> : MethodApplicabilityChecker
    {
        readonly Func<T1, T2, bool> ArgsAreValid;

        public override IEnumerable<Type> InputTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
            }
        }

        public MethodApplicabilityChecker(Func<T1, T2, bool> argsAreValid)
        {
            ArgsAreValid = argsAreValid;
        }

        protected override bool _TestArgs(IEnumerable<object> args)
        {
            var a = args.ToArray();
            return ArgsAreValid((T1)a[0], (T2)a[1]);
        }
    }

    internal class MethodApplicabilityChecker<T1, T2, T3> : MethodApplicabilityChecker
    {
        readonly Func<T1, T2, T3, bool> ArgsAreValid;

        public override IEnumerable<Type> InputTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
                yield return typeof(T3);
            }
        }

        public MethodApplicabilityChecker(Func<T1, T2, T3, bool> argsAreValid)
        {
            ArgsAreValid = argsAreValid;
        }

        protected override bool _TestArgs(IEnumerable<object> args)
        {
            var a = args.ToArray();
            return ArgsAreValid((T1)a[0], (T2)a[1], (T3)a[2]);
        }
    }

    internal class MethodApplicabilityChecker<T1, T2, T3, T4> : MethodApplicabilityChecker
    {
        readonly Func<T1, T2, T3, T4, bool> ArgsAreValid;

        public override IEnumerable<Type> InputTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
                yield return typeof(T3);
                yield return typeof(T4);
            }
        }

        public MethodApplicabilityChecker(Func<T1, T2, T3, T4, bool> argsAreValid)
        {
            ArgsAreValid = argsAreValid;
        }

        protected override bool _TestArgs(IEnumerable<object> args)
        {
            var a = args.ToArray();
            return ArgsAreValid((T1)a[0], (T2)a[1], (T3)a[2], (T4)a[3]);
        }
    }

    internal class MethodApplicabilityChecker<T1, T2, T3, T4, T5> : MethodApplicabilityChecker
    {
        readonly Func<T1, T2, T3, T4, T5, bool> ArgsAreValid;

        public override IEnumerable<Type> InputTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
                yield return typeof(T3);
                yield return typeof(T4);
                yield return typeof(T5);
            }
        }

        public MethodApplicabilityChecker(Func<T1, T2, T3, T4, T5, bool> argsAreValid)
        {
            ArgsAreValid = argsAreValid;
        }

        protected override bool _TestArgs(IEnumerable<object> args)
        {
            var a = args.ToArray();
            return ArgsAreValid((T1)a[0], (T2)a[1], (T3)a[2], (T4)a[3], (T5)a[4]);
        }
    }

    internal class MethodApplicabilityChecker<T1, T2, T3, T4, T5, T6> : MethodApplicabilityChecker
    {
        readonly Func<T1, T2, T3, T4, T5, T6, bool> ArgsAreValid;

        public override IEnumerable<Type> InputTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
                yield return typeof(T3);
                yield return typeof(T4);
                yield return typeof(T5);
                yield return typeof(T6);
            }
        }

        public MethodApplicabilityChecker(Func<T1, T2, T3, T4, T5, T6, bool> argsAreValid)
        {
            ArgsAreValid = argsAreValid;
        }

        protected override bool _TestArgs(IEnumerable<object> args)
        {
            var a = args.ToArray();
            return ArgsAreValid((T1)a[0], (T2)a[1], (T3)a[2], (T4)a[3], (T5)a[4], (T6)a[5]);
        }
    }
}
