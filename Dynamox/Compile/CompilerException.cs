using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Compile
{
    public class CompilerException : Exception
    {
        public CompilerException(Type parentType, string message)
            : this(parentType, message, null)
        {
        }

        public CompilerException(Type parentType, string message, Exception innerException)
            : base(BuildErrorMessage(parentType, message), innerException)
        {

        }

        static string BuildErrorMessage(Type parentType, string message)
        {
            return "Error compiling mock class for " + parentType + Environment.NewLine + message;
        }
    }
}
