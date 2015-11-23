using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests.Builders
{
    public partial class TestBuilder
    {
        public static void Run(ITestModule module)
        {
            foreach (var test in module)
                Run(test, module);
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

            if (false /*throws.Length*/)
            {
                try
                {
                    work();
                }
                catch (Exception e)
                {
                    //TODO: cache exception
                }
            }
            else
            {
                work();
            }

            foreach (var ass in arrange.Take((tmp = arrange.IndexOf(arrange.FirstOrDefault(a => !a._UseBaseAssert))) == -1 ? int.MaxValue : (tmp + 1))
                .Reverse().SelectMany(a => a._Assert))
                ass(arranger.Copy(), result);

            foreach (var thr in arrange.Take((tmp = arrange.IndexOf(arrange.FirstOrDefault(a => !a._UseBaseAssert))) == -1 ? int.MaxValue : (tmp + 1))
                .Reverse().SelectMany(a => a._Throws))
            {
                if (exception == null)
                    throw new InvalidOperationException();

                if (!thr.Key.IsAssignableFrom(exception.GetType()))
                    throw new InvalidOperationException();

                thr.Value(arranger.Copy(), exception);
            }
        }
    }
}
