using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectTests.Tests.Features.Mocks;

namespace DirectTests.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new Ensure().Method_NotOk_AfterProperty();
        }
    }
}
