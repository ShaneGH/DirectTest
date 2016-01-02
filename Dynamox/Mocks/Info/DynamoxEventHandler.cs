using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks.Info
{
    public class DynamoxEventHandler : DynamoxEventHandlerBase
    {
        readonly Action EventHandler;

        public override IEnumerable<Type> EventArgTypes
        {
            get { return Enumerable.Empty<Type>(); }
        }

        public DynamoxEventHandler(Action eventHandler)
        {
            EventHandler = eventHandler;
        }

        public override void Invoke(IEnumerable<object> withArgs)
        {
            EventHandler();
        }
    }

    public class DynamoxEventHandler<T> : DynamoxEventHandlerBase
    {
        readonly Action<T> EventHandler;

        public override IEnumerable<Type> EventArgTypes
        {
            get
            {
                yield return typeof(T);
            }
        }

        public DynamoxEventHandler(Action<T> eventHandler)
        {
            EventHandler = eventHandler;
        }

        public override void Invoke(IEnumerable<object> withArgs)
        {
            var args = withArgs.ToArray();
            EventHandler((T)args[0]);
        }
    }

    public class DynamoxEventHandler<T1, T2> : DynamoxEventHandlerBase
    {
        readonly Action<T1, T2> EventHandler;

        public override IEnumerable<Type> EventArgTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
            }
        }

        public DynamoxEventHandler(Action<T1, T2> eventHandler)
        {
            EventHandler = eventHandler;
        }

        public override void Invoke(IEnumerable<object> withArgs)
        {
            var args = withArgs.ToArray();
            EventHandler((T1)args[0], (T2)args[1]);
        }
    }

    public class DynamoxEventHandler<T1, T2, T3> : DynamoxEventHandlerBase
    {
        readonly Action<T1, T2, T3> EventHandler;

        public override IEnumerable<Type> EventArgTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
                yield return typeof(T3);
            }
        }

        public DynamoxEventHandler(Action<T1, T2, T3> eventHandler)
        {
            EventHandler = eventHandler;
        }

        public override void Invoke(IEnumerable<object> withArgs)
        {
            var args = withArgs.ToArray();
            EventHandler((T1)args[0], (T2)args[1], (T3)args[2]);
        }
    }

    public class DynamoxEventHandler<T1, T2, T3, T4> : DynamoxEventHandlerBase
    {
        readonly Action<T1, T2, T3, T4> EventHandler;

        public override IEnumerable<Type> EventArgTypes
        {
            get
            {
                yield return typeof(T1);
                yield return typeof(T2);
                yield return typeof(T3);
                yield return typeof(T4);
            }
        }

        public DynamoxEventHandler(Action<T1, T2, T3, T4> eventHandler)
        {
            EventHandler = eventHandler;
        }

        public override void Invoke(IEnumerable<object> withArgs)
        {
            var args = withArgs.ToArray();
            EventHandler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3]);
        }
    }

    public class DynamoxEventHandler<T1, T2, T3, T4, T5> : DynamoxEventHandlerBase
    {
        readonly Action<T1, T2, T3, T4, T5> EventHandler;

        public override IEnumerable<Type> EventArgTypes
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

        public DynamoxEventHandler(Action<T1, T2, T3, T4, T5> eventHandler)
        {
            EventHandler = eventHandler;
        }

        public override void Invoke(IEnumerable<object> withArgs)
        {
            var args = withArgs.ToArray();
            EventHandler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4]);
        }
    }

    public class DynamoxEventHandler<T1, T2, T3, T4, T5, T6> : DynamoxEventHandlerBase
    {
        readonly Action<T1, T2, T3, T4, T5, T6> EventHandler;

        public override IEnumerable<Type> EventArgTypes
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

        public DynamoxEventHandler(Action<T1, T2, T3, T4, T5, T6> eventHandler)
        {
            EventHandler = eventHandler;
        }

        public override void Invoke(IEnumerable<object> withArgs)
        {
            var args = withArgs.ToArray();
            EventHandler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5]);
        }
    }

    public class DynamoxEventHandler<T1, T2, T3, T4, T5, T6, T7> : DynamoxEventHandlerBase
    {
        readonly Action<T1, T2, T3, T4, T5, T6, T7> EventHandler;

        public override IEnumerable<Type> EventArgTypes
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

        public DynamoxEventHandler(Action<T1, T2, T3, T4, T5, T6, T7> eventHandler)
        {
            EventHandler = eventHandler;
        }

        public override void Invoke(IEnumerable<object> withArgs)
        {
            var args = withArgs.ToArray();
            EventHandler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6]);
        }
    }

    public class DynamoxEventHandler<T1, T2, T3, T4, T5, T6, T7, T8> : DynamoxEventHandlerBase
    {
        readonly Action<T1, T2, T3, T4, T5, T6, T7, T8> EventHandler;

        public override IEnumerable<Type> EventArgTypes
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

        public DynamoxEventHandler(Action<T1, T2, T3, T4, T5, T6, T7, T8> eventHandler)
        {
            EventHandler = eventHandler;
        }

        public override void Invoke(IEnumerable<object> withArgs)
        {
            var args = withArgs.ToArray();
            EventHandler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7]);
        }
    }

    public class DynamoxEventHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9> : DynamoxEventHandlerBase
    {
        readonly Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> EventHandler;

        public override IEnumerable<Type> EventArgTypes
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

        public DynamoxEventHandler(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> eventHandler)
        {
            EventHandler = eventHandler;
        }

        public override void Invoke(IEnumerable<object> withArgs)
        {
            var args = withArgs.ToArray();
            EventHandler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7], (T9)args[8]);
        }
    }

    public class DynamoxEventHandler<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : DynamoxEventHandlerBase
    {
        readonly Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> EventHandler;

        public override IEnumerable<Type> EventArgTypes
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

        public DynamoxEventHandler(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> eventHandler)
        {
            EventHandler = eventHandler;
        }

        public override void Invoke(IEnumerable<object> withArgs)
        {
            var args = withArgs.ToArray();
            EventHandler((T1)args[0], (T2)args[1], (T3)args[2], (T4)args[3], (T5)args[4], (T6)args[5], (T7)args[6], (T8)args[7], (T9)args[8], (T10)args[9]);
        }
    }
}
