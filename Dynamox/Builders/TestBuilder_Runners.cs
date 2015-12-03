using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Builders
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

        static IEnumerable<T> Filter<T>(IList<TestBuilder> orderedSet, Func<TestBuilder, bool> stopAt, Func<TestBuilder, IEnumerable<T>> selectMany)
        {
            int tmp;
            return orderedSet
                .Take((tmp = orderedSet.IndexOf(orderedSet.FirstOrDefault(stopAt))) == -1 ? int.MaxValue : (tmp + 1))
                .Reverse().SelectMany(selectMany);
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
                foreach (var act in Filter(arrange, a => !a._UseBaseAct, a => a._Act))
                    result = act(arranger);
            };
            
            var throws = Filter(arrange, a => !a._UseBaseThrows, a => a._Throws);

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

            if (arranger.ShouldHaveBeenCalled.Any())
                throw new InvalidOperationException("methods not called\n" + string.Join("\n", arranger.ShouldHaveBeenCalled));  //TODO
            

            foreach (var ass in Filter(arrange, a => !a._UseBaseAssert, a => a._Assert))
                ass(arranger, result);

            foreach (var thr in throws)
            {
                if (exception == null)
                    throw new InvalidOperationException();  //TODO

                if (!thr.Key.IsAssignableFrom(exception.GetType()))
                    throw new InvalidOperationException();//TODO

                thr.Value(arranger, exception);
            }
        }
    }
}
