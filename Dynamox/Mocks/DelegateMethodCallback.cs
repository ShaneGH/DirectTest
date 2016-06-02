using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    class DelegateMethodCallback : MethodCallbackBase
    {
        Action<IEnumerable<object>> Action;
        IEnumerable<Type> ArgTypes;

        public override IEnumerable<Type> InputTypes
        {
            get
            {
                return ArgTypes;
            }
        }

        public DelegateMethodCallback(Action<IEnumerable<object>> action, IEnumerable<Type> argTypes)
        {
            Action = action;
            ArgTypes = argTypes;
        }

        protected override void _Do(IEnumerable<object> args)
        {
            Action(args);
        }
    }
}
