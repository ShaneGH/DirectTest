using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    internal class EqualityMethodApplicabilityChecker : MethodApplicabilityChecker
    {
        public override IEnumerable<Type> InputTypes
        {
            get
            {
                return Args.Select(a => a == null ? typeof(AnyValue) : 
                    (a is AnyValue ? (a as AnyValue).OfType : a.GetType()));
            }
        }

        readonly IEnumerable<object> Args;

        public EqualityMethodApplicabilityChecker(IEnumerable<object> args)
        {
            Args = args.ToArray();
        }

        protected override bool _TestArgs(IEnumerable<object> args)
        {
            var a1 = args.ToArray();
            var a2 = Args.ToArray();
            for (var i = 0; i < a1.Length; i++)
            {
                if (a2[i] is AnyValue) ;
                else if (a1[i] == null)
                {
                    if (a2[i] != null)
                        return false;
                }
                else if (!a1[i].Equals(a2[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
