using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;

namespace Dynamox.Compile
{
    public abstract class ILBuilderBase
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

    public abstract class IlBuilder : ILBuilderBase
    {
        protected readonly TypeBuilder ToType;
        protected readonly FieldInfo ObjBase;

        public IlBuilder(TypeBuilder toType, FieldInfo objBase)
        {
            ToType = toType;
            ObjBase = objBase;
        }

        public static MethodAttributes? GetAccessAttr(MethodInfo forMethod)
        {
            if (forMethod.IsPublic)
                return MethodAttributes.Public;
            else if (forMethod.IsFamilyOrAssembly)
                return MethodAttributes.FamORAssem;
            else if (forMethod.IsFamily)
                return MethodAttributes.Family;
            else if (forMethod.IsAssembly)
                return MethodAttributes.Assembly;
            else if (forMethod.IsFamilyAndAssembly)
                return MethodAttributes.FamANDAssem;
            else if (forMethod.IsPrivate)
                return MethodAttributes.Private;

            return null;
        }
    }
}