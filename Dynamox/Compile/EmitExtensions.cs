using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;

namespace Dynamox.Compile
{
    public static class EmitExtensions
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

        public static void SetArrayElement(this ILGenerator body, LocalBuilder array, int index, Action getValue)
        {
            body.Emit(OpCodes.Ldloc, array);
            _SetArrayElement(body, index, getValue);
        }

        public static void SetArrayElement(this ILGenerator body, OpCode arrayLocation, int index, Action getValue)
        {
            body.Emit(arrayLocation);
            _SetArrayElement(body, index, getValue);
        }

        static void _SetArrayElement(ILGenerator body, int index, Action getValue)
        {
            body.Emit(OpCodes.Ldc_I4, index);
            getValue();
            body.Emit(OpCodes.Stelem_Ref);
        }

        static readonly MethodInfo GetTypeFromHandle = TypeUtils.GetMethod(() => Type.GetTypeFromHandle(default(RuntimeTypeHandle)));
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

        static readonly MethodInfo Console_WriteLine = TypeUtils.GetMethod(() => Console.WriteLine(default(object)));
        public static void ConsoleWriteLine(this ILGenerator body)
        {
            body.Emit(OpCodes.Call, Console_WriteLine);
        }

        public static void ConsoleWriteLine(this ILGenerator body, Type writeLineType)
        {
            body.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new[] { writeLineType }));
        }

        static readonly MethodInfo _CheckNull = TypeUtils.GetMethod(() => EmitExtensions.CheckNull(default(object)));
        public static bool CheckNull(object input)
        {
            return input == null;
        }

        public static void CheckForNull(this ILGenerator body, LocalBuilder variable, Label? ifNull = null, Label? ifNotNull = null)
        {
            body.Emit(OpCodes.Ldloc, variable);
            body.CheckTopOfStackForNull(ifNull, ifNotNull);
        }

        public static void CheckForNull(this ILGenerator body, OpCode variableLocation, Label? ifNull = null, Label? ifNotNull = null)
        {
            body.Emit(variableLocation);
            body.CheckTopOfStackForNull(ifNull, ifNotNull);
        }

        public static void CheckTopOfStackForNull(this ILGenerator body, Label? ifNull = null, Label? ifNotNull = null) 
        {
            if (!ifNull.HasValue && !ifNotNull.HasValue)
                throw new InvalidOperationException("You must specify either a true or false execution path");

            body.Emit(OpCodes.Call, _CheckNull);

            if (ifNull.HasValue)
            {
                body.Emit(OpCodes.Brtrue, ifNull.Value);
                if (ifNotNull.HasValue)
                    body.Emit(OpCodes.Brtrue, ifNotNull.Value);
            }
            else if (ifNotNull.HasValue)
            {
                body.Emit(OpCodes.Brfalse, ifNotNull.Value);
            }
        }
    }
}
