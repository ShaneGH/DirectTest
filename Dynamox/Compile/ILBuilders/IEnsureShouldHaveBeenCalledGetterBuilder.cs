using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;
using Dynamox.Mocks.Info;

namespace Dynamox.Compile.ILBuilders
{
    /// <summary>
    /// Build a getter for the IEnsure.ShouldHaveBeenCalled property
    /// </summary>
    public class IEnsureShouldHaveBeenCalledGetterBuilder : PropertyBuilder
    {
        static readonly PropertyInfo IEnsureShouldHaveBeenCalled = typeof(IEnsure).GetProperty("ShouldHaveBeenCalled");

        /// <summary>
        /// The type of the IEnsure.ShouldHaveBeenCalled property
        /// </summary>
        public static Type PropertyType
        {
            get
            {
                return IEnsureShouldHaveBeenCalled.PropertyType;
            }
        }

        /// <summary>
        /// The name of the IEnsure.ShouldHaveBeenCalled property
        /// </summary>
        public static string Name
        {
            get
            {
                return IEnsureShouldHaveBeenCalled.Name;
            }
        }

        public IEnsureShouldHaveBeenCalledGetterBuilder(TypeBuilder toType, FieldInfo objBase)
            : base(toType, objBase, IEnsureShouldHaveBeenCalled.GetMethod)
        {
            AddInterfaceMethodsExplicitly = true;
        }

        protected override LocalBuilder CallMockedMethod(LocalBuilder generics, LocalBuilder args, LocalBuilder methodOut)
        {
            var ifResult = Body.DeclareLocal(typeof(bool));

            // methodOut = this.ObjectBase.ShouldHaveBeenCalled
            Body.Emit(OpCodes.Ldarg_0);
            Body.Emit(OpCodes.Ldfld, ObjBase);
            Body.Emit(OpCodes.Call, ParentMethod);
            Body.Emit(OpCodes.Stloc, methodOut);

            // ifResult = true
            Body.Emit(OpCodes.Ldc_I4_1);
            Body.Emit(OpCodes.Stloc, ifResult);

            return ifResult;
        }
    }
}