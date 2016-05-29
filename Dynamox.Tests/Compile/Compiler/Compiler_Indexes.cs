using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Compile;
using Dynamox.Mocks;
using Dynamox.Mocks.Info;
using NUnit.Framework;
using COMPILER = Dynamox.Compile.Compiler;

namespace Dynamox.Tests.Compile.Compiler
{
    [TestFixture]
    public class Compiler_Indexes
    {
        public abstract class Indexes
        {
            public abstract int this[string val] { get; set; }
            public virtual int this[bool val] { get { throw new NotImplementedException(); } set { } }
        }

        [Test]
        public void IndexesTests()
        {
            var values = new MockBuilder();
            ((dynamic)values)["hello"] = 22;
            ((dynamic)values)[true] = 44;

            var subject = (Indexes)
                COMPILER.Compile(typeof(Indexes)).GetConstructors()[0].Invoke(new object[]
                { 
                    new ObjectBase(Dx.Settings, values) 
                });

            Assert.AreEqual(subject["hello"], 22);
            subject["hello"] = 33;
            Assert.AreEqual(subject["hello"], 33);
            Assert.AreEqual(subject[true], 44);
        }
    }
}