using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;

namespace Dynamox.Compile.ILBuilders
{
    /// <summary>
    /// Build a setter for an indexed property which overrides a virtual parent property
    /// </summary>
    public class VirtualIndexSetterBuilder : PropertyBuilder
    {
        static readonly MethodInfo MethodArg_Length = TypeUtils.GetProperty<MethodArg[], int>(a => a.Length).GetMethod;
        static readonly MethodInfo Take = TypeUtils.GetMethod(() => Enumerable.Take(default(IEnumerable<MethodArg>), 0), true);
        readonly string PropertyName;

        public VirtualIndexSetterBuilder(TypeBuilder toType, FieldInfo objBase, PropertyInfo parentProperty)
            : base(toType, objBase, parentProperty.SetMethod)
        {
            if (parentProperty.SetMethod == null)
                throw new InvalidOperationException("Cannot overrided setter"); //TODO

            PropertyName = parentProperty.Name;
        }

        protected override LocalBuilder CallMockedMethod(LocalBuilder generics, LocalBuilder args, LocalBuilder methodOut)
        {
            var ifResult = Body.DeclareLocal(typeof(bool));
            var indexParams = Body.DeclareLocal(typeof(IEnumerable<MethodArg>));
            var last = Body.DeclareLocal(typeof(int));

            //last = args.Length - 1;
            Body.Emit(OpCodes.Ldloc, args);
            Body.Emit(OpCodes.Call, MethodArg_Length);
            Body.Emit(OpCodes.Ldc_I4_1);
            Body.Emit(OpCodes.Sub);
            Body.Emit(OpCodes.Stloc, last);

            //indexParams = Enumerable.Take(args, last);
            Body.Emit(OpCodes.Ldloc, args);
            Body.Emit(OpCodes.Ldloc, last);
            Body.Emit(OpCodes.Call, Take);
            Body.Emit(OpCodes.Stloc, indexParams);

            // this.ObjectBase.SetIndex(indexParams, args[last].Arg);
            Body.Emit(OpCodes.Ldarg_0);
            Body.Emit(OpCodes.Ldfld, ObjBase);
            Body.Emit(OpCodes.Ldloc, indexParams);
            Body.Emit(OpCodes.Ldloc, args);
            Body.Emit(OpCodes.Ldloc, last);
            Body.Emit(OpCodes.Ldelem_Ref);
            Body.Emit(OpCodes.Ldfld, MethodArg_Arg);
            Body.Emit(OpCodes.Call, ObjectBase.Reflection.TrySetIndex);

            // ifResult = topOfStack == 1
            Body.Emit(OpCodes.Ldc_I4_1);
            Body.Emit(OpCodes.Ceq);
            Body.Emit(OpCodes.Stloc, ifResult);

            return ifResult;
        }
    }
}