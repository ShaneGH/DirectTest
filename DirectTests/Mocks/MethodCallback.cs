using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests.Mocks
{
    //TODO: MORE!!!!!

    internal class MethodCallback : MethodCallbackBase
    {
        readonly Action Callback;

        public override IEnumerable<Type> InputTypes
        {
            get
            {
                yield break;
            }
        }

        public MethodCallback(Action callback)
        {
            Callback = callback;
        }

        protected override void _Do(IEnumerable<object> args)
        {
            Callback();
        }
    }

    internal class MethodCallback<T> : MethodCallbackBase
    {
        readonly Action<T> Callback;

        public override IEnumerable<Type> InputTypes
        {
            get
            {
                yield return typeof(T);
            }
        }

        public MethodCallback(Action<T> callback)
        {
            Callback = callback;
        }

        protected override void _Do(IEnumerable<object> args)
        {
            var a = args.ToArray();
            Callback((T)a[0]);
        }
    }

    internal class MethodCallback<T1, T2> : MethodCallbackBase
    {
        readonly Action<T1, T2> Callback;

        public override IEnumerable<Type> InputTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
            }
        }

        public MethodCallback(Action<T1, T2> callback)
        {
            Callback = callback;
        }

        protected override void _Do(IEnumerable<object> args)
        {
            var a = args.ToArray();
            Callback((T1)a[0], (T2)a[1]);
        }
    }

    internal class MethodCallback<T1, T2, T3> : MethodCallbackBase
    {
        readonly Action<T1, T2, T3> Callback;

        public override IEnumerable<Type> InputTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
                yield return typeof(T3);
            }
        }

        public MethodCallback(Action<T1, T2, T3> callback)
        {
            Callback = callback;
        }

        protected override void _Do(IEnumerable<object> args)
        {
            var a = args.ToArray();
            Callback((T1)a[0], (T2)a[1], (T3)a[2]);
        }
    }

    internal class MethodCallback<T1, T2, T3, T4> : MethodCallbackBase
    {
        readonly Action<T1, T2, T3, T4> Callback;

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

        public MethodCallback(Action<T1, T2, T3, T4> callback)
        {
            Callback = callback;
        }

        protected override void _Do(IEnumerable<object> args)
        {
            var a = args.ToArray();
            Callback((T1)a[0], (T2)a[1], (T3)a[2], (T4)a[3]);
        }
    }

    internal class MethodCallback<T1, T2, T3, T4, T5> : MethodCallbackBase
    {
        readonly Action<T1, T2, T3, T4, T5> Callback;

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

        public MethodCallback(Action<T1, T2, T3, T4, T5> callback)
        {
            Callback = callback;
        }

        protected override void _Do(IEnumerable<object> args)
        {
            var a = args.ToArray();
            Callback((T1)a[0], (T2)a[1], (T3)a[2], (T4)a[3], (T5)a[4]);
        }
    }

    internal class MethodCallback<T1, T2, T3, T4, T5, T6> : MethodCallbackBase
    {
        readonly Action<T1, T2, T3, T4, T5, T6> Callback;

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

        public MethodCallback(Action<T1, T2, T3, T4, T5, T6> callback)
        {
            Callback = callback;
        }

        protected override void _Do(IEnumerable<object> args)
        {
            var a = args.ToArray();
            Callback((T1)a[0], (T2)a[1], (T3)a[2], (T4)a[3], (T5)a[4], (T6)a[5]);
        }
    }

    internal class MethodCallback<T1, T2, T3, T4, T5, T6, T7> : MethodCallbackBase
    {
        readonly Action<T1, T2, T3, T4, T5, T6, T7> Callback;

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
                yield return typeof(T7);
            }
        }

        public MethodCallback(Action<T1, T2, T3, T4, T5, T6, T7> callback)
        {
            Callback = callback;
        }

        protected override void _Do(IEnumerable<object> args)
        {
            var a = args.ToArray();
            Callback((T1)a[0], (T2)a[1], (T3)a[2], (T4)a[3], (T5)a[4], (T6)a[5], (T7)a[6]);
        }
    }

    internal class MethodCallback<T1, T2, T3, T4, T5, T6, T7, T8> : MethodCallbackBase
    {
        readonly Action<T1, T2, T3, T4, T5, T6, T7, T8> Callback;

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
                yield return typeof(T7);
                yield return typeof(T8);
            }
        }

        public MethodCallback(Action<T1, T2, T3, T4, T5, T6, T7, T8> callback)
        {
            Callback = callback;
        }

        protected override void _Do(IEnumerable<object> args)
        {
            var a = args.ToArray();
            Callback((T1)a[0], (T2)a[1], (T3)a[2], (T4)a[3], (T5)a[4], (T6)a[5], (T7)a[6], (T8)a[7]);
        }
    }

    internal class MethodCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9> : MethodCallbackBase
    {
        readonly Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> Callback;

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
                yield return typeof(T7);
                yield return typeof(T8);
                yield return typeof(T9);
            }
        }

        public MethodCallback(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> callback)
        {
            Callback = callback;
        }

        protected override void _Do(IEnumerable<object> args)
        {
            var a = args.ToArray();
            Callback((T1)a[0], (T2)a[1], (T3)a[2], (T4)a[3], (T5)a[4], (T6)a[5], (T7)a[6], (T8)a[7], (T9)a[8]);
        }
    }

    internal class MethodCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : MethodCallbackBase
    {
        readonly Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Callback;

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
                yield return typeof(T7);
                yield return typeof(T8);
                yield return typeof(T9);
                yield return typeof(T10);
            }
        }

        public MethodCallback(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> callback)
        {
            Callback = callback;
        }

        protected override void _Do(IEnumerable<object> args)
        {
            var a = args.ToArray();
            Callback((T1)a[0], (T2)a[1], (T3)a[2], (T4)a[3], (T5)a[4], (T6)a[5], (T7)a[6], (T8)a[7], (T9)a[8], (T10)a[9]);
        }
    }
}
