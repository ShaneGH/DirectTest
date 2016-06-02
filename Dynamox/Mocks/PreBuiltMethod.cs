using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    public abstract class PreBuiltMethod : IMethod
    {
        readonly Delegate Method;

        public PreBuiltMethod(Delegate method)
        {
            Method = method;
        }

        public object Invoke(IEnumerable<object> arguments)
        {
            return Method.DynamicInvoke(arguments.ToArray());
        }

        public abstract IEnumerable<Type> ArgTypes { get; }

        public abstract Type ReturnType { get; }

        public bool Ensured { get; private set; }

        public IMethod DxEnsure()
        {
            Ensured = true;
            return this;
        }
    }
}
