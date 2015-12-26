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
        public readonly DxSettings TestSettings;
        public readonly string ModuleName;
        readonly List<TestBuilder> Tests = new List<TestBuilder>();

        public TestModule(string moduleName = null, DxSettings settings = null)
        {
            ModuleName = moduleName ?? "Unnamed";
            TestSettings = settings ?? new DxSettings();
        }

        public TestModule(ITest test)
            : this(settings: test.Settings)
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
            var test = new TestBuilder(name, TestSettings);
            Tests.Add(test);
            return test;
        }

        public void Add(ITest test)
        {
            Tests.Add(test.Builder);
        }
    }
}
