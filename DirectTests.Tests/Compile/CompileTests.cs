using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectTests.Compile;
using DirectTests.Mocks;
using NUnit.Framework;

namespace DirectTests.Tests.Compile
{
    [TestFixture]
    public class CompileTests
    {
        public abstract class LostOfProperties
        {
            int _Prop1 = 77;
            public virtual int Prop1 { get { return _Prop1; } set { _Prop1 = value; } }
            internal int Prop2 { get; set; }
            protected abstract int Prop3 { get; set; }
            private int Prop4 { get; set; }
            protected virtual internal int Prop5 { get; set; }
            public virtual int Prop6 { get; private set; }
            public virtual int Prop7 { get; protected set; }
            public virtual int Prop8 { get; internal set; }
            public abstract int Prop9 { get; }
            public abstract int Prop10 { set; }
            protected abstract internal int Prop11 { get; set; }
        }

        [Test]
        public void LotsOfProperties()
        {
            var subject = (LostOfProperties)
                Compiler.Compile(typeof(LostOfProperties)).GetConstructors()[0].Invoke(new object[] { new ObjectBase(null) });

            //TODO assert
        }
    }
}
