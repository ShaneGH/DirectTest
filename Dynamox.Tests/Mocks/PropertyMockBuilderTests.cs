using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;
using NUnit.Framework;

namespace Dynamox.Tests.Mocks
{
    [TestFixture]
    public class PropertyMockBuilderTests
    {
        [Test]
        public void Casting() 
        {
            var subject = new PropertyMockBuilder<string>();

            Assert.True(subject is IPropertyMockBuilder<object>);
        }
    }
}
