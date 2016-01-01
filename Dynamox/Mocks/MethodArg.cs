using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    /// <summary>
    /// Input to ObjctBase get/invoke methods
    /// </summary>
    public class MethodArg
    {
        public readonly Type ArgType;
        public readonly string ArgName;
        public object Arg;

        public MethodArg(object arg, Type argType, string argName)
        {
            Arg = arg;
            ArgType = argType;
            ArgName = argName;
        }
    }

    public interface IMethodArg<out T> 
    {
        T Arg { get; }
    }

    /// <summary>
    /// Input to ObjctBase get/invoke methods. Not really necessary, but removes the need to cast/box in IL
    /// </summary>
    public class MethodArg<T> : MethodArg, IMethodArg<T>
    {
        public MethodArg(T arg, string argName)
            : base(arg, typeof(T), argName)
        {
        }

        public MethodArg(string argName)
            : this(default(T), argName)
        {
        }

        T IMethodArg<T>.Arg
        {
            get { return (T)Arg; }
        }
    }
}
