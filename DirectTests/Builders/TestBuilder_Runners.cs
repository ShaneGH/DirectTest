using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests.Builders
{
    public partial class TestBuilder
    {
        public static void Run(ITestModule module, string singleTest = null)
        {
            if (singleTest == null)
            {
                foreach (var test in module)
                    Run(test, module);

                return;
            }

            var selectedTest = module.FirstOrDefault(t => t.TestName == singleTest);
            if (selectedTest == null)
                throw new InvalidOperationException("Test not found");    //TODO

            Run(selectedTest, module);
        }

        static void Run(TestBuilder test, ITestModule module)
        {
            string last;
            var arrange = new List<TestBuilder>(new[] { test });
            while ((last = arrange.Last()._BasedOn) != null)
            {
                var current = module.FirstOrDefault(t => t.TestName == last);
                if (current == null)
                    throw new InvalidOperationException("There is no test named \"" + last + "\" in this group.");

                if (arrange.Contains(current))
                    throw new InvalidOperationException();

                arrange.Add(current);
            }

            int tmp;
            var arranger = new TestArranger();
            foreach (var arr in (arrange as IEnumerable<TestBuilder>).Reverse().SelectMany(a => a._Arrange))
            {
                arr(arranger);
                arranger.SetAllSettingsToDefault();
            }

            object result = null;
            Exception exception = null;
            Action work = () =>
            {
                foreach (var act in arrange.Take((tmp = arrange.IndexOf(arrange.FirstOrDefault(a => !a._UseBaseAct))) == -1 ? int.MaxValue : (tmp + 1))
                    .Reverse().SelectMany(a => a._Act))
                    result = act(arranger.Copy());
            };

            var throws = arrange.Take((tmp = arrange.IndexOf(arrange.FirstOrDefault(a => !a._UseBaseThrows))) == -1 ? int.MaxValue : (tmp + 1))
                .Reverse().SelectMany(a => a._Throws);

            if (throws.Any())
            {
                try
                {
                    work();
                }
                catch (Exception e)
                {
                    exception = e;
                }
            }
            else
            {
                work();
            }

            foreach (var ass in arrange.Take((tmp = arrange.IndexOf(arrange.FirstOrDefault(a => !a._UseBaseAssert))) == -1 ? int.MaxValue : (tmp + 1))
                .Reverse().SelectMany(a => a._Assert))
                ass(arranger.Copy(), result);

            foreach (var thr in throws)
            {
                if (exception == null)
                    throw new InvalidOperationException();  //TODO

                if (!thr.Key.IsAssignableFrom(exception.GetType()))
                    throw new InvalidOperationException();//TODO

                thr.Value(arranger.Copy(), exception);
            }
        }
    }
}
