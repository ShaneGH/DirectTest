using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    public class MockedMethodNotCalledException : Exception
    {
        public MockedMethodNotCalledException(IEnumerable<string> errors)
            : base(BuildMessage(errors))
        {

        }

        static string BuildMessage(IEnumerable<string> errors)
        {
            return string.Join(Environment.NewLine, new[] { "The following method(s) were not called:" }.Concat(errors));
        }
    }
}
