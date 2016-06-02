using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    public class PreBuiltMethod<TReturn> : PreBuiltMethod
    {
        public PreBuiltMethod(Func<TReturn> method)
            : base(method)
        {
        }

        public override IEnumerable<Type> ArgTypes
        {
            get
            {
                yield break;
            }
        }

        public override Type ReturnType
        {
            get { return typeof(TReturn); }
        }
    }

    public class PreBuiltMethod<T, TReturn> : PreBuiltMethod
    {
        public PreBuiltMethod(Func<T, TReturn> method)
            : base(method)
        {
        }

        public override IEnumerable<Type> ArgTypes
        {
            get
            {
                yield return typeof(T);
            }
        }

        public override Type ReturnType
        {
            get { return typeof(TReturn); }
        }
    }

    public class PreBuiltMethod<T1, T2, TReturn> : PreBuiltMethod
    {
        public PreBuiltMethod(Func<T1, T2, TReturn> method)
            : base(method)
        {
        }

        public override IEnumerable<Type> ArgTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
            }
        }

        public override Type ReturnType
        {
            get { return typeof(TReturn); }
        }
    }

    public class PreBuiltMethod<T1, T2, T3, TReturn> : PreBuiltMethod
    {
        public PreBuiltMethod(Func<T1, T2, T3, TReturn> method)
            : base(method)
        {
        }

        public override IEnumerable<Type> ArgTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
                yield return typeof(T3);
            }
        }

        public override Type ReturnType
        {
            get { return typeof(TReturn); }
        }
    }

    public class PreBuiltMethod<T1, T2, T3, T4, TReturn> : PreBuiltMethod
    {
        public PreBuiltMethod(Func<T1, T2, T3, T4, TReturn> method)
            : base(method)
        {
        }

        public override IEnumerable<Type> ArgTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
                yield return typeof(T3);
                yield return typeof(T4);
            }
        }

        public override Type ReturnType
        {
            get { return typeof(TReturn); }
        }
    }

    public class PreBuiltMethod<T1, T2, T3, T4, T5, TReturn> : PreBuiltMethod
    {
        public PreBuiltMethod(Func<T1, T2, T3, T4, T5, TReturn> method)
            : base(method)
        {
        }

        public override IEnumerable<Type> ArgTypes
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

        public override Type ReturnType
        {
            get { return typeof(TReturn); }
        }
    }

    public class PreBuiltMethod<T1, T2, T3, T4, T5, T6, TReturn> : PreBuiltMethod
    {
        public PreBuiltMethod(Func<T1, T2, T3, T4, T5, T6, TReturn> method)
            : base(method)
        {
        }

        public override IEnumerable<Type> ArgTypes
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

        public override Type ReturnType
        {
            get { return typeof(TReturn); }
        }
    }

    public class PreBuiltMethod<T1, T2, T3, T4, T5, T6, T7, TReturn> : PreBuiltMethod
    {
        public PreBuiltMethod(Func<T1, T2, T3, T4, T5, T6, T7, TReturn> method)
            : base(method)
        {
        }

        public override IEnumerable<Type> ArgTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
                yield return typeof(T3);
                yield return typeof(T4);
                yield return typeof(T5);
                yield return typeof(T6);
                yield return typeof(T7);
            }
        }

        public override Type ReturnType
        {
            get { return typeof(TReturn); }
        }
    }

    public class PreBuiltMethod<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> : PreBuiltMethod
    {
        public PreBuiltMethod(Func<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> method)
            : base(method)
        {
        }

        public override IEnumerable<Type> ArgTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
                yield return typeof(T3);
                yield return typeof(T4);
                yield return typeof(T5);
                yield return typeof(T6);
                yield return typeof(T7);
                yield return typeof(T8);
            }
        }

        public override Type ReturnType
        {
            get { return typeof(TReturn); }
        }
    }

    public class PreBuiltMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn> : PreBuiltMethod
    {
        public PreBuiltMethod(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn> method)
            : base(method)
        {
        }

        public override IEnumerable<Type> ArgTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
                yield return typeof(T3);
                yield return typeof(T4);
                yield return typeof(T5);
                yield return typeof(T6);
                yield return typeof(T7);
                yield return typeof(T8);
                yield return typeof(T9);
            }
        }

        public override Type ReturnType
        {
            get { return typeof(TReturn); }
        }
    }

    public class PreBuiltMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn> : PreBuiltMethod
    {
        public PreBuiltMethod(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn> method)
            : base(method)
        {
        }

        public override IEnumerable<Type> ArgTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
                yield return typeof(T3);
                yield return typeof(T4);
                yield return typeof(T5);
                yield return typeof(T6);
                yield return typeof(T7);
                yield return typeof(T8);
                yield return typeof(T9);
                yield return typeof(T10);
            }
        }

        public override Type ReturnType
        {
            get { return typeof(TReturn); }
        }
    }
}
