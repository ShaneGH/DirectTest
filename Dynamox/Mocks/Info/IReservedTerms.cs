using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks.Info
{
    /// <summary>
    /// The reserved terms for a mock statement
    /// </summary>
    public interface IReservedTerms
    {
        string Returns { get; }
        string Ensure { get; }
        string Clear { get; }
        string Do { get; }
        string As { get; }
        string Constructor { get; }
        string Out { get; }
    }
}
