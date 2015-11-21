using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests
{
    public interface IArrange
    {
        IAct Arrange(Action<dynamic> arrange);
    }

    public interface IBasedOn : IArrange
    {
        IArrange BasedOn(string basedOn);
    }
}