using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks.Info;

namespace Dynamox.Mocks
{
    /// <summary>
    /// Represents a mocked method
    /// </summary>
    public interface IMethodAssert
    {
        bool TestArgs(IEnumerable<MethodArg> args);

        bool TestInputArgTypes(IEnumerable<MethodArg> inputArgs);

        bool CanMockMethod(MethodInfo method);

        IEnumerable<Type> ArgTypes { get; }

        List<OutArg> OutParamValues { get; set; }
    }
}
