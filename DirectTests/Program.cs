using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TestBuilder.Test("My test")
            //    .BasedOn("An ancient test")
                .Arrange(bag =>
                {
                    bag.entityNumber = 2;
                    bag.commandId = 55;
                    bag.repository.GetEntity(bag.commandId).Returns(new ClassToTest.Entity { Number = bag.entityNumber });
                })

                .UseParentAct()
                .Act(bag =>
                {
                    bag.constructorNumber = 55;
                    ClassToTest subject = new ClassToTest((ClassToTest.IRepo1)bag.repository, bag.constructorNumber);
                    return subject.Execute(new ClassToTest.Command { Id = bag.commandId });
                })

                .SkipParentAssert()
                .Assert((bag, result) => 
                {
                    if (result.Number != 99)
                        throw new InvalidOperationException();
                });

            TestBuilder.Run("My test");
        }
    }

    public class ClassToTest
    {
        readonly IRepo1 Repository;
        readonly int ANumber;

        public ClassToTest(IRepo1 repo1, int aNumber)
        {
            Repository = repo1;
            ANumber = aNumber;
        }

        public Result Execute(Command command) 
        {
            var entity = Repository.GetEntity(command.Id);
            return new Result { Number = entity.Number * ANumber };
        }

        public class Command
        {
            public int Id { get; set; }
        }

        public class Result
        {
            public int Number { get; set; }
        }

        public interface IRepo1 
        {
            Entity GetEntity(int id);
        }

        public class Entity 
        {
            public int Number { get; set; }
        }
    }
}
