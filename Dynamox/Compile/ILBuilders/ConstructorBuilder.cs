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
    /// Build a constructor with initial property setters
    /// </summary>
    internal class ConstructorBuilder : NewBlockILBuilder
    {
        readonly ConstructorInfo Constructor;
        readonly TypeOverrideDescriptor Descriptor;
        ILGenerator MethodBody { get; set; }

        public ConstructorBuilder(TypeBuilder toType, FieldInfo objBase, ConstructorInfo constructor, TypeOverrideDescriptor descriptor)
            : base(toType, objBase)
        {
            Constructor = constructor;
            Descriptor = descriptor;
        }

        protected override void _Build()
        {
            var args = new[] { typeof(ObjectBase) }
                .Concat(Constructor.GetParameters().Select(p => p.ParameterType)).ToArray();
            var con = TypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, args);

            MethodBody = con.GetILGenerator();

            var ret = MethodBody.DefineLabel();

            // Set objectBase
            // arg 0 is "this"
            MethodBody.Emit(OpCodes.Ldarg_0);
            // arg 1 is objBase
            MethodBody.Emit(OpCodes.Ldarg_1);
            // this.Field = arg1;
            MethodBody.Emit(OpCodes.Stfld, ObjBase);

            // Call base constructor
            MethodBody.Emit(OpCodes.Ldarg_0);
            for (var i = 1; i < args.Length; i++)
                MethodBody.Emit(OpCodes.Ldarg_S, (short)(i + 1));
            MethodBody.Emit(OpCodes.Call, Constructor);

            // if (ObjectBase.Settings.SetNonVirtualPropertiesOrFields != true) return;
            MethodBody.Emit(OpCodes.Ldarg_1);
            MethodBody.Emit(OpCodes.Ldfld, typeof(ObjectBase).GetField("Settings"));
            MethodBody.Emit(OpCodes.Call, typeof(DxSettings).GetProperty("SetNonVirtualPropertiesOrFields").GetMethod);
            MethodBody.Emit(OpCodes.Ldc_I4_1);
            MethodBody.Emit(OpCodes.Ceq);
            MethodBody.Emit(OpCodes.Brfalse, ret);

            BuildSetters();
            MethodBody.Emit(OpCodes.Br, ret);

            MethodBody.MarkLabel(ret);
            MethodBody.Emit(OpCodes.Ret);
        }

        void BuildSetters()
        {
            foreach (var field in Descriptor.SettableFields.Select(f => new FieldSetterBuilder(MethodBody, f)))
            {
                field.Build();
            }

            foreach (var property in Descriptor.SettableProperties
                .Where(p => !p.GetIndexParameters().Any())
                .Select(p => new PropertySetterBuilder(MethodBody, p)))
            {
                property.Build();
            }

            foreach (var property in Descriptor.SettableProperties
                .Where(p => p.GetIndexParameters().Any())
                .Select(p => new IndexedPropertySetterBuilder(MethodBody, p)))
            {
                property.Build();
            }
        }
    }
}
