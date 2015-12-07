using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Compile;
using Dynamox.Mocks;
using Dynamox.Tests.Compile;
using Dynamox.Tests.Features.Mocks;
using Dynamox.Tests.Mocks;

namespace Dynamox.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new TypeOverrideDescriptorTests().PropertiesTest();

            var obj = (X1)Activator.CreateInstance(Compiler2.Compile(typeof(X1)), new object[]{new ObjectBase(
                new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
                {
                    {"Hello", "bbb"}
                }))});

            Console.WriteLine(obj.Hello);
            Console.ReadKey();
        }
    }



    public abstract class X1
    {
        public X1() { }
        public X1(string hello)
        {
            Hello = hello;
        }
        internal abstract string Hello { get; set; }
    }

    public class X2 : X1
    {
        public X2() { }

        internal override string Hello
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
