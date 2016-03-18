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
        string DxReturns { get; }
        string DxEnsure { get; }
        string DxClear { get; }
        string DxDo { get; }
        string DxAs { get; }
        string DxConstructor { get; }
        string DxOut { get; }
    }
}
