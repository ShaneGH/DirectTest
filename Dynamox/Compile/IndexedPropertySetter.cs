using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;

namespace Dynamox.Compile
{
    public class IndexedPropertySetter : ILBuilderBase
    {
        static readonly FieldInfo Arg = typeof(MethodArg).GetField("Arg");
        static readonly MethodInfo ElementAt = typeof(Enumerable).GetMethod("ElementAt", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod(new[] { typeof(MethodArg) });
        static readonly MethodInfo Current = typeof(IEnumerator<IEnumerable<MethodArg>>).GetProperty("Current").GetMethod;
        static readonly MethodInfo GetEnumerator = typeof(IEnumerable<IEnumerable<MethodArg>>).GetMethod("GetEnumerator");
        static readonly MethodInfo GetIndex = typeof(ObjectBase).GetMethod("GetIndex");
        static readonly MethodInfo MoveNext = typeof(IEnumerator).GetMethod("MoveNext");
        static readonly MethodInfo GetMockedIndexKeys = typeof(ObjectBase).GetMethod("GetMockedIndexKeys");

        readonly ILGenerator MethodBody;
        readonly PropertyInfo Property;

        public IndexedPropertySetter(ILGenerator methodBody, PropertyInfo property)
        {
            MethodBody = methodBody;
            Property = property;
        }

        protected override void _Build()
        {
            var allParamaters = Property.SetMethod.GetParameters().Select(p => p.ParameterType);
            var indexTypes = allParamaters.Take(allParamaters.Count() - 1).ToArray();
            var propertyType = allParamaters.Last();
            var value = MethodBody.DeclareLocal(propertyType);
            var keys = MethodBody.DeclareLocal(typeof(IEnumerable<MethodArg>));
            var startLoop = MethodBody.DefineLabel();
            var endLoop = MethodBody.DefineLabel();
            var vars = indexTypes.Select(it => MethodBody.DeclareLocal(it)).ToArray();

            var types = new[] { propertyType };

            // var indexes = new Type[i];
            var indexes = MethodBody.DeclareLocal(typeof(Type[]));
            MethodBody.Emit(OpCodes.Ldc_I4, indexTypes.Count());
            MethodBody.Emit(OpCodes.Newarr, typeof(Type));
            MethodBody.Emit(OpCodes.Stloc, indexes);

            for (var i = 0; i < indexTypes.Length; i++)
            {
                // indexes[i] = typeof(TKey);
                MethodBody.Emit(OpCodes.Ldloc, indexes);
                MethodBody.Emit(OpCodes.Ldc_I4, i);
                MethodBody.TypeOf(indexTypes[i]);
                MethodBody.Emit(OpCodes.Stelem_Ref);
            }

            // var result = ObjectBase.GetMockedIndexKeys<TProperty>(indexes)
            var result = MethodBody.DeclareLocal(typeof(IEnumerable<IEnumerable<MethodArg>>));
            MethodBody.Emit(OpCodes.Ldarg_1);
            MethodBody.Emit(OpCodes.Ldloc, indexes);
            MethodBody.Emit(OpCodes.Call, GetMockedIndexKeys.MakeGenericMethod(types));
            MethodBody.Emit(OpCodes.Stloc, result);

            // var enumerable = result.GetEnumerator();
            var enumerable = MethodBody.DeclareLocal(typeof(IEnumerator<IEnumerable<MethodArg>>));
            MethodBody.Emit(OpCodes.Ldloc, result);
            MethodBody.Emit(OpCodes.Callvirt, GetEnumerator);
            MethodBody.Emit(OpCodes.Stloc, enumerable);

            MethodBody.MarkLabel(startLoop);

            // if (!enumerable.MoveNext())  GO TO END
            MethodBody.Emit(OpCodes.Ldloc, enumerable);
            MethodBody.Emit(OpCodes.Callvirt, MoveNext);
            MethodBody.Emit(OpCodes.Ldc_I4_0);
            MethodBody.Emit(OpCodes.Ceq);
            MethodBody.Emit(OpCodes.Brtrue, endLoop);

            // keys = enumerable.Current
            MethodBody.Emit(OpCodes.Ldloc, enumerable);
            MethodBody.Emit(OpCodes.Callvirt, Current);
            MethodBody.Emit(OpCodes.Stloc, keys);

            // value = ObjectBase.GetIndex<TIndexed>(IEnumerable<MethodArg> indexValues)
            MethodBody.Emit(OpCodes.Ldarg_1);
            MethodBody.Emit(OpCodes.Ldloc, keys);
            MethodBody.Emit(OpCodes.Call, GetIndex.MakeGenericMethod(new[] { propertyType }));
            MethodBody.Emit(OpCodes.Stloc, value);
            
            for (var i = 0; i < vars.Length; i++)
            {
                //var0 = (T)keys.ElementAt(0).Arg;
                MethodBody.Emit(OpCodes.Ldloc, keys);
                MethodBody.Emit(OpCodes.Ldc_I4, i);
                MethodBody.Emit(OpCodes.Call, ElementAt);
                MethodBody.Emit(OpCodes.Ldfld, Arg);
                if (indexTypes[i].IsValueType)
                    MethodBody.Emit(OpCodes.Unbox_Any, indexTypes[i]);
                else
                    MethodBody.Emit(OpCodes.Castclass, indexTypes[i]);

                MethodBody.Emit(OpCodes.Stloc, vars[i]);
            }

            // this.set_Property(key1, key2.....etc, value);
            MethodBody.Emit(OpCodes.Ldarg_0);
            for (var i = 0; i < vars.Length; i++)
            {
                MethodBody.Emit(OpCodes.Ldloc, vars[i]);
            }

            MethodBody.Emit(OpCodes.Ldloc, value);
            MethodBody.Emit(OpCodes.Call, Property.SetMethod);

            // GO TO START
            MethodBody.Emit(OpCodes.Br, startLoop);

            MethodBody.MarkLabel(endLoop);
        }
    }
}
