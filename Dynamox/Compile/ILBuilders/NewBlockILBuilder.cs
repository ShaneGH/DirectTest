using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Compile.ILBuilders
{
    /// <summary>
    /// Base class for building IL where the method body will be defined by the class
    /// </summary>
    public abstract class NewBlockILBuilder : ILBuilder
    {
        protected readonly TypeBuilder TypeBuilder;
        protected readonly FieldInfo ObjBase;

        public NewBlockILBuilder(TypeBuilder toType, FieldInfo objBase)
        {
            TypeBuilder = toType;
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
