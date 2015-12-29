using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks.Info
{
    /// <summary>
    /// Represents an out or ref paramater with either a name or index
    /// </summary>
    public class OutArg
    {
        //TODO: should be 2 classes with overriden functionality

        public readonly int Index;
        public readonly string Name;
        public readonly object Value;

        public OutArg(int index, object value)
        {
            Index = index;
            Value = value;
            Name = null;
        }

        public OutArg(string name, object value)
        {
            Index = -1;
            Value = value;
            Name = name;
        }
    }
}
