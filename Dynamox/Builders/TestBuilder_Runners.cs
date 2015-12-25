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
                throw new InvalidOperationException("Test not found");    //TODE

            Run(selectedTest, module);
        }

        /// <summary>
        /// Beginning at the end, of the set, return all elements until the element after the element which satifies the condition [stopAt]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="orderedSet"></param>
        /// <param name="stopAt"></param>
        /// <returns></returns>
        static IEnumerable<TestBuilder> Filter(List<TestBuilder> orderedSet, Func<TestBuilder, bool> stopAt)
        {
            var first = orderedSet.LastOrDefault(stopAt);
            return first == null ? 
                orderedSet : 
                orderedSet.Skip(orderedSet.LastIndexOf(first));
        }

        static void Run(TestBuilder test, ITestModule module)
        {
            string last;
            var testTree = new List<TestBuilder>(new[] { test });
            while ((last = testTree.First()._BasedOn) != null)
            {
                var current = module.FirstOrDefault(t => t.TestName == last);
                if (current == null)
                    throw new InvalidOperationException("There is no test named \"" + last + "\" in this group.");

                if (testTree.Contains(current))
                    throw new InvalidOperationException();

                testTree.Insert(0, current);
            }

            var arranger = new TestArranger(DxSettings.GlobalSettings);
            foreach (var arr in Filter(testTree, a => !a._UseParentArrange).SelectMany(a => a._Arrange))
            {
                arr(arranger);
                arranger.SetAllSettingsToDefault();
            }

            object result = null;
            Exception exception = null;
            Action work = () =>
            {
                foreach (var act in Filter(testTree, a => !a._UseBaseAct).SelectMany(a => a._Act))
                    result = act(arranger);
            };

            var throws = Filter(testTree, a => !a._UseBaseThrows).SelectMany(a => a._Throws);

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
                throw new InvalidOperationException("methods not called\n" + string.Join("\n", arranger.ShouldHaveBeenCalled));  //TODE


            foreach (var ass in Filter(testTree, a => !a._UseBaseAssert).SelectMany(a => a._Assert))
                ass(arranger, result);

            foreach (var thr in throws)
            {
                if (exception == null)
                    throw new InvalidOperationException();  //TODE

                if (!thr.Key.IsAssignableFrom(exception.GetType()))
                    throw new InvalidOperationException();//TODE

                thr.Value(arranger, exception);
            }
        }
    }
}
