using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;
using Dynamox.Mocks.Info;
using NUnit.Framework;

namespace Dynamox.Tests.Mocks.Info
{
    [TestFixture]
    public class PropertyMockBuilderTests
    {
        [Test]
        public void Casting() 
        {
            var subject = new PropertyMockBuilder<string>();

            Assert.True(subject is IPropertyMockBuilder<string>);
            Assert.True(subject is IPropertyMockBuilder<object>);
        }
    }
}
