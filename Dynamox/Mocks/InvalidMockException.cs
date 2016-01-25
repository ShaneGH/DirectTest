using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    public class InvalidMockException : Exception
    {
        public InvalidMockException(string message)
            : base(message)
        {
        }

        public InvalidMockException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
