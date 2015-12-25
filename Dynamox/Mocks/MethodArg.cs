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
    public class MethodArg
    {
        public readonly Type ArgType;
        public readonly object Arg;

        public MethodArg(object arg, Type argType)
        {
            Arg = arg;
            ArgType = argType;
        }
    }

    public class MethodArg<T> : MethodArg
    {
        public MethodArg(T arg)
            : base(arg, typeof(T))
        {
        }

        public MethodArg()
            : this(default(T))
        {
        }
    }
}
