using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectTests.Compile;
using DirectTests.Tests.Features.Mocks;
using DirectTests.Tests.Mocks;

namespace DirectTests.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new ObjectBaseTests().GetSetIndexes();
            Compiler.Compile(typeof(X));
        }
    }

    public class X
    {
        public virtual string this[Program key, int yy]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual string this[string key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
