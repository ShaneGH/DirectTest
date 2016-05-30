using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks.Info
{
    internal class EnsuredProperty
    {
        public readonly object Value;

        public EnsuredProperty(object value) 
        {
            Value = value;
        }
    }
}
