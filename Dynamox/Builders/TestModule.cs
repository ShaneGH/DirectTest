using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Builders
{
    public interface ITestModule : IEnumerable<TestBuilder>
    {
        IBasedOn Add(string name);
        void Add(ITest test);
    }

    public class TestModule : ITestModule
    {
        readonly string ModuleName;
        readonly List<TestBuilder> Tests = new List<TestBuilder>();

        public TestModule(string moduleName = null)
        {
            ModuleName = moduleName ?? "Unnamed";
        }

        public TestModule(ITest test)
        {
            Tests.Add(test.Builder);
            ModuleName = Tests[0].TestName;
        }

        IEnumerator<TestBuilder> IEnumerable<TestBuilder>.GetEnumerator()
        {
            return Tests.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Tests.GetEnumerator();
        }

        public IBasedOn Add(string name)
        {
            var test = new TestBuilder(name);
            Tests.Add(test);
            return test;
        }

        public void Add(ITest test)
        {
            Tests.Add(test.Builder);
        }
    }
}
