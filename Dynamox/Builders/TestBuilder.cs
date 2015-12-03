using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Builders
{
    public partial class TestBuilder : ITest, IBasedOn, IArrange, IAct, IAssert
    {
        string _BasedOn { get; set; }

        bool _UseBaseAct = true;
        bool _UseBaseAssert = false;
        bool _UseBaseThrows = false;

        readonly List<Action<dynamic>> _Arrange = new List<Action<dynamic>>();
        readonly List<Func<dynamic, object>> _Act = new List<Func<dynamic, object>>();
        readonly List<Action<dynamic, object>> _Assert = new List<Action<dynamic, object>>();
        readonly List<KeyValuePair<Type, Action<dynamic, Exception>>> _Throws = new List<KeyValuePair<Type, Action<dynamic, Exception>>>();

        public readonly string TestName;

        public TestBuilder(string testName)
        {
            TestName = testName;
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

        public IFor Subject(ConstructorInfo constructor)
        {
            return new SimpleTestBuilder(this).Subject(constructor);
        }

        public IFor<TSubject> Subject<TSubject>(Expression<Func<TSubject>> constructor)
        {
            return new SimpleTestBuilder(this).Subject(constructor);
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

        public IAssert SkipParentThrows(bool skipParentThrows = true)
        {
            _UseBaseThrows = !skipParentThrows;
            return this;
        }

        public ITest Assert(Action<dynamic> result)
        {
            _Assert.Add((a, b) => result(a));
            return this;
        }

        public ITest Throws<TException>()
            where TException : Exception
        {
            return Throws<TException>((a, b) => { });
        }

        public ITest Throws<TException>(Action<dynamic, TException> result)
            where TException : Exception
        {
            _Throws.Add(new KeyValuePair<Type, Action<dynamic, Exception>>(typeof(TException), (a, b) => result(a, (TException)b)));
            return this;
        }

        public void Run()
        {
            Framework.Run(this);
        }

        TestBuilder ITest.Builder
        {
            get { return this; }
        }
    }
}
