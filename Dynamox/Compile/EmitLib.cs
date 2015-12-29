using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;

namespace Dynamox.Compile
{
    public static class EmitLib
    {
        public static void LoadArrayElement(this ILGenerator body, LocalBuilder array, int index)
        {
            body.Emit(OpCodes.Ldloc, array);
            _LoadArrayElement(body, index);
        }

        public static void LoadArrayElement(this ILGenerator body, OpCode arrayLocation, int index)
        {
            body.Emit(arrayLocation);
            _LoadArrayElement(body, index);
        }

        static void _LoadArrayElement(ILGenerator body, int index)
        {
            body.Emit(OpCodes.Ldc_I4, index);
            body.Emit(OpCodes.Ldelem_Ref);
        }

        static readonly MethodInfo GetTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle");
        public static void TypeOf(this ILGenerator body, Type type)
        {
            body.Emit(OpCodes.Ldtoken, type);
            body.Emit(OpCodes.Call, GetTypeFromHandle);
        }

        public static LocalBuilder CreateArray(this ILGenerator body, Type arrayType, int length)
        {
            var array = body.DeclareLocal(arrayType.MakeArrayType());
            body.Emit(OpCodes.Ldc_I4, length);
            body.Emit(OpCodes.Newarr, arrayType);
            body.Emit(OpCodes.Stloc, array);

            return array;
        }
    }
}
