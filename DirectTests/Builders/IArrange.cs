using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DirectTests.Dynamic;

namespace DirectTests.Builders
{
    public interface IArrange : ITest
    {
        IAct Arrange(Action<dynamic> arrange);

        IFor Subject(ConstructorInfo constructor);

        //IFor<TSubject> Subject<TSubject>(Expression<Func<TSubject>> constructor);
    }

    public interface IBasedOn : IArrange
    {
        IArrange BasedOn(string basedOn);
    }

    public interface IParameterizedArrange<TReturnValue> : ITest
    {
        IAssert<TReturnValue> Arrange(Action<ITestData> arrange);
    }
}