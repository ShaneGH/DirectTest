using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    /// <summary>
    /// Input to ObjctBase get methods
    /// </summary>
    public abstract class MethodArg
    {
        public abstract Type ArgType { get; }
        public readonly object Arg;

        public MethodArg(object arg)
        {
            Arg = arg;
        }
    }

    public class MethodArg<T> : MethodArg
    {
        public override Type ArgType
        {
            get { return typeof(T); }
        }

        public MethodArg(T arg)
            : base(arg)
        {
        }

        public MethodArg()
            : base(null)
        {
        }
    }
}
