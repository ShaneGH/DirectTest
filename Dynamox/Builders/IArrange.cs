using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Dynamic;

namespace Dynamox.Builders
{
    public interface IArrange : ITest
    {
        IArrange UseParentArrange(bool useParentArrange = true);

        IAct Arrange(Action<dynamic> arrange);

        IFor Subject(ConstructorInfo constructor);

        IFor<TSubject> Subject<TSubject>(Expression<Func<TSubject>> constructor);
    }

    public interface IBasedOn : IArrange
    {
        IArrange BasedOn(string basedOn);
    }

    public interface IParameterizedArrange : ITest
    {
        IAssert Arrange(Action<ITestData> arrange);
    }

    public interface IParameterizedArrange<TReturnValue> : IParameterizedArrange
    {
        new IAssert<TReturnValue> Arrange(Action<ITestData> arrange);
    }
}