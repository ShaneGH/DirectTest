using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;

namespace Dynamox.Compile.ILBuilders
{
    /// <summary>
    /// A base class for building IL. Provides simple mechanisms to ensure it is only built once
    /// </summary>
    public abstract class ILBuilder
    {
        protected abstract void _Build();

        bool Built = false;
        readonly object BuildLock = new object();
        public void Build()
        {
            lock (BuildLock)
            {
                if (Built)
                    return;

                Built = true;

                _Build();
            }
        }
    }
}