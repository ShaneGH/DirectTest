using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Dynamox.Tests
{
    [SetUpFixture]
    public class TestSuiteSetup
    {
        [SetUp]
        public void RunBeforeAnyTests()
        {
            Dx.Settings.TestForInvalidMocks = true;
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
        }
    }
}
