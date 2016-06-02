using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    public abstract class PreBuiltVoidMethodBase: PreBuiltMethod
    {
        public PreBuiltVoidMethodBase(Delegate method)
            : base(method)
        {
        }

        public override Type ReturnType
        {
            get { return typeof(object); }
        }
    }

    public class PreBuiltVoidMethod: PreBuiltVoidMethodBase
    {
        public PreBuiltVoidMethod(Action method)
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
    }

    public class PreBuiltVoidMethod<T>: PreBuiltVoidMethodBase
    {
        public PreBuiltVoidMethod(Action<T> method)
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
    }

    public class PreBuiltVoidMethod<T1, T2>: PreBuiltVoidMethodBase
    {
        public PreBuiltVoidMethod(Action<T1, T2> method)
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
    }

    public class PreBuiltVoidMethod<T1, T2, T3>: PreBuiltVoidMethodBase
    {
        public PreBuiltVoidMethod(Action<T1, T2, T3> method)
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
    }

    public class PreBuiltVoidMethod<T1, T2, T3, T4>: PreBuiltVoidMethodBase
    {
        public PreBuiltVoidMethod(Action<T1, T2, T3, T4> method)
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
    }

    public class PreBuiltVoidMethod<T1, T2, T3, T4, T5>: PreBuiltVoidMethodBase
    {
        public PreBuiltVoidMethod(Action<T1, T2, T3, T4, T5> method)
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
    }

    public class PreBuiltVoidMethod<T1, T2, T3, T4, T5, T6>: PreBuiltVoidMethodBase
    {
        public PreBuiltVoidMethod(Action<T1, T2, T3, T4, T5, T6> method)
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
    }

    public class PreBuiltVoidMethod<T1, T2, T3, T4, T5, T6, T7>: PreBuiltVoidMethodBase
    {
        public PreBuiltVoidMethod(Action<T1, T2, T3, T4, T5, T6, T7> method)
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
    }

    public class PreBuiltVoidMethod<T1, T2, T3, T4, T5, T6, T7, T8>: PreBuiltVoidMethodBase
    {
        public PreBuiltVoidMethod(Action<T1, T2, T3, T4, T5, T6, T7, T8> method)
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
    }

    public class PreBuiltVoidMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9>: PreBuiltVoidMethodBase
    {
        public PreBuiltVoidMethod(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> method)
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
    }

    public class PreBuiltVoidMethod<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>: PreBuiltVoidMethodBase
    {
        public PreBuiltVoidMethod(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> method)
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
    }
}
