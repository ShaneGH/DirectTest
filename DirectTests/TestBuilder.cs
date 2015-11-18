using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests
{
    public interface IBasedOn : IArrange
    {
        IArrange BasedOn(string basedOn);
    }

    public interface IArrange
    {
        IAct Arrange(Action<dynamic> arrange);
    }

    public interface IAct
    {
        IAct UseParentAct(bool useParentAct = true);
        IAssert Act(Action<dynamic> action);
        IAssert<TResult> Act<TResult>(Func<dynamic, TResult> action);
    }

    public interface IAssert
    {
        IAssert SkipParentAssert(bool skipParentAssert = true);
        void Assert(Action<dynamic> result);
        void Throws<TException>(Action<dynamic, TException> result)
            where TException : Exception;
    }

    public interface IAssert<TResult> : IAssert
    {
        new IAssert<TResult> SkipParentAssert(bool skipParentAssert = true);
        void Assert(Action<dynamic, TResult> result);
    }

    public class TestBuilder : IBasedOn, IArrange, IAct, IAssert
    {
        string _BasedOn { get; set; }

        bool _UseBaseAct = true;
        bool _UseBaseAssert = false;

        readonly List<Action<dynamic>> _Arrange = new List<Action<dynamic>>();
        readonly List<Func<dynamic, object>> _Act = new List<Func<dynamic, object>>();
        readonly List<Action<dynamic, object>> _Assert = new List<Action<dynamic, object>>();
        readonly List<KeyValuePair<Type, Action<dynamic, Exception>>> _Throws = new List<KeyValuePair<Type,Action<dynamic,Exception>>>();

        public readonly string TestName;

        public TestBuilder(string testName)
        {
            TestName = testName;
        }

        public static IBasedOn Test(string testName)
        {
            return _Builders[testName] = new TestBuilder(testName);
        }

        #region TEST CODE, remove

        static readonly Dictionary<string, TestBuilder> _Builders = new Dictionary<string, TestBuilder>();
        public static void Run(string test)
        {
            string last;
            var arrange = new List<TestBuilder>(new[] { _Builders[test] });
            while ((last = arrange.Last()._BasedOn) != null)
            {
                if (!_Builders.ContainsKey(last))
                    throw new InvalidOperationException("There is no test named \"" + last + "\" in this group.");

                if (arrange.Contains(_Builders[last]))
                    throw new InvalidOperationException();

                arrange.Add(_Builders[last]);
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
            foreach (var act in arrange.Take((tmp = arrange.IndexOf(arrange.FirstOrDefault(a => !a._UseBaseAct))) == -1 ? int.MaxValue : (tmp + 1))
                .Reverse().SelectMany(a => a._Act))
            {
                try
                {
                    result = act(arranger.Copy());
                }
                catch (Exception e)
                {
                    exception = e;
                }
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

        #endregion

        static void TEST()
        {
            Test("My test")
                .Arrange(testBag =>
                {
                })
                .Act(testBag =>
                {
                })
                .Assert(testBag =>
                {
                });
        }

        public IArrange BasedOn(string basedOn)
        {
            _BasedOn = basedOn;
            return this;
        }

        public IAct Arrange(Action<dynamic> arrange)
        {
            _Arrange.Add(arrange);
            return this;
        }

        public IAct UseParentAct(bool useParentAct = true)
        {
            _UseBaseAct = useParentAct;
            return this;
        }

        public IAssert Act(Action<dynamic> action)
        {
            _Act.Add(a =>
            {
                action(a);
                return null;
            });
            return this;
        }

        public IAssert<TResult> Act<TResult>(Func<dynamic, TResult> action)
        {
            _Act.Add(a => action(a));
            return new Asserter<TResult>(this);
        }

        public IAssert SkipParentAssert(bool skipParentAssert = true)
        {
            _UseBaseAssert = !skipParentAssert;
            return this;
        }

        public void Assert(Action<dynamic> result)
        {
            _Assert.Add((a, b) => result(a));
        }

        public void Throws<TException>(Action<dynamic, TException> result) where TException : Exception
        {
            _Throws.Add(new KeyValuePair<Type, Action<dynamic, Exception>>(typeof(TException), (a, b) => result(a, (TException)b)));
        }

        private class Asserter<TTestResult> : IAssert<TTestResult>
        {
            private readonly TestBuilder BasedOn;

            public Asserter(TestBuilder basedOn)
            {
                BasedOn = basedOn;
            }

            public void Assert(Action<dynamic, TTestResult> result)
            {
                //TODO, catch cast errors
                BasedOn._Assert.Add((a, b) => result(a, (TTestResult)b));
            }

            public void Assert(Action<dynamic> result)
            {
                BasedOn.Assert(result);
            }

            public void Throws<TException>(Action<dynamic, TException> result) where TException : Exception
            {
                BasedOn.Throws<TException>(result);
            }

            public IAssert<TTestResult> SkipParentAssert(bool skipParentAssert = true)
            {
                BasedOn._UseBaseAssert = !skipParentAssert;
                return this;
            }

            IAssert IAssert.SkipParentAssert(bool skipParentAssert)
            {
                return BasedOn.SkipParentAssert(skipParentAssert);
            }
        }
    }
}
