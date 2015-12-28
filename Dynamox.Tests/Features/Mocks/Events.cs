using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Dynamox.Tests.Features.Mocks
{
    [TestFixture]
    public class Events
    {
        public interface ICurrentTest
        {
            event EventHandler SomethingDid;
        }

        public abstract class CurrentTest1
        {
            public abstract event EventHandler SomethingDid;
        }

        public abstract class CurrentTest2
        {
            public virtual event EventHandler SomethingDid;

            public CurrentTest2() 
            {
                SomethingDid(null, null);
            }
        }

        public abstract class CurrentTest3 : CurrentTest2
        {
            public override event EventHandler SomethingDid;

            public CurrentTest3() 
            {
                SomethingDid(null, null);
            }
        }

        [Test]
        public void SmokeTest()
        {
            ICurrentTest mock = Dx.Mock();

            mock.SomethingDid += (object sender, EventArgs e) =>
            {
                throw new NotImplementedException();
            };
        }
    }
}