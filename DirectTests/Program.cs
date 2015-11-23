using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests
{
    public class Program
    {
        public abstract class IRepo1
        {
            public abstract Add.Entity GetEntity(int id);
        }

        public abstract class IRepoFactory
        {
            public abstract IRepo1 GetRepo(bool whatever);
        }

        public class Add
        {
            readonly IRepoFactory RepositoryFactory;
            readonly IRepo1 Repo;

            public Add(IRepoFactory repo1)
            {
                RepositoryFactory = repo1;
            }

            public Add(IRepo1 repo1)
            {
                Repo = repo1;
            }

            public int Execute1(int id, int add)
            {
                var entity = Repo.GetEntity(id);
                return entity.Number *= add;
            }

            public int Execute2(int id, int add)
            {
                var repo = RepositoryFactory.GetRepo(true);
                var entity = repo.GetEntity(id);
                return entity.Number *= add;
            }

            public void Execute3(int id, int add)
            {
                Execute1(id, add);
            }

            public class Entity
            {
                public int Id { get; set; }
                public int Number { get; set; }
            }
        }

        public static void Main(string[] args)
        {
            Framework.Test("My test")
                .Arrange(bag =>
                {
                    bag.entityNumber = 2;
                    bag.id = 55;
                    bag.repo
                        .GetEntity(Framework.Method<int>(a => a == bag.id))
                        .Return(new Add.Entity { Id = bag.id, Number = bag.entityNumber });
                })

                .UseParentAct()
                .Act(bag =>
                {
                    bag.add = 55;
                    var subject = new Add(bag.repo.As<IRepo1>());
                    return subject.Execute1((int)bag.id, (int)bag.add);
                })

                .SkipParentAssert()
                .Assert((bag, result) =>
                {
                    if (result != 110)
                        throw new InvalidOperationException();
                })

                .Run();
        }
    }
    public interface IRepo1
    {
        Add.Entity GetEntity(int id);
    }

    public interface IRepoFactory
    {
        IRepo1 GetRepo(bool whatever);
    }

    public class Add
    {
        readonly IRepoFactory RepositoryFactory;
        readonly IRepo1 Repo;

        public Add(IRepoFactory repo1)
        {
            RepositoryFactory = repo1;
        }

        public Add(IRepo1 repo1)
        {
            Repo = repo1;
        }

        public int Execute1(int id, int add)
        {
            var entity = Repo.GetEntity(id);
            return entity.Number * add;
        }

        public int Execute2(int id, int add)
        {
            var repo = RepositoryFactory.GetRepo(true);
            var entity = repo.GetEntity(id);
            return entity.Number * add;
        }

        public class Entity
        {
            public int Id { get; set; }
            public int Number { get; set; }
        }
    }
}



//        //    new TestBuilderBase<ClassToTest, int>("Base test")
//        //        .Constructor(() => new ClassToTest(null, ""))
//        //        .Method(x => x.MethodToTest(null, 4))
//        //        .Build(testCase =>
//        //        {
//        //            testCase.Configuration = new Configuration();

//        //            testCase.Args.arg1 = new object();
//        //            testCase.CArgs.arg2.Val1.Val2.Val3(testCase.Any(), 2, testCase.Args.arg1)
//        //                .Ensure()
//        //                .Returns();

//        //            testCase.CArgs.arg3(new ArgKeywords { Ensure = "Banana" }).Val1.Val2.Val3(44)
//        //                .Do(testCase.Action<int>(a => { }))
//        //                .Banana();

//        //            testCase.CArgs.arg4.Val3(testCase.Assert<object, string>((a, b) => true));

//        //            testCase.Args.arg5 = new object();
//        //            testCase.Args.arg5.DoSomething()
//        //                .Returns(4);
//        //        })
//        //        .Assert((a, b) =>
//        //        {
//        //        })
//        //        .Assert((a, b, c) =>
//        //        {
//        //        });