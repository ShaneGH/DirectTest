using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;

namespace Dynamox.Compile
{
    /// <summary>
    /// Build a method for a dynamic type based on a method in the parent class
    /// Dumb cass which is not not thread safe
    /// </summary>
    public class AbstractIndexSetterBuilder : PropertyBuilder
    {
        readonly string PropertyName;

        public AbstractIndexSetterBuilder(TypeBuilder toType, FieldInfo objBase, PropertyInfo parentProperty)
            : base(toType, objBase, parentProperty.SetMethod)
        {
            if (parentProperty.SetMethod == null)
                throw new InvalidOperationException("Cannot overrided setter"); //TODO

            PropertyName = parentProperty.Name;
        }

        static MethodInfo MethodArg_Length;
        static MethodInfo Take;
        protected override LocalBuilder CallMockedMethod(LocalBuilder generics, LocalBuilder args, LocalBuilder methodOut)
        {
            if (Take == null)
            {
                Expression<Func<IEnumerable<MethodArg>>> takeTmp = () => Enumerable.Take(default(IEnumerable<MethodArg>), 0);
                Take = (takeTmp.Body as MethodCallExpression).Method;

                MethodArg_Length = typeof(MethodArg[]).GetProperty("Length").GetMethod;
            }

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
            Body.Emit(OpCodes.Call, ObjectBase.Reflection.SetIndex);

            // ifResult = true
            Body.Emit(OpCodes.Ldc_I4_1);
            Body.Emit(OpCodes.Stloc, ifResult);

            return ifResult;
        }
    }
}