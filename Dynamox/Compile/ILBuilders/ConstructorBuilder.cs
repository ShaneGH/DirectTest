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

            //if (IsIEventChain)
            //    AddIEventChainFunctionality();

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

        //void AddIEventChainFunctionality()
        //{
        //    SubscribeToBubbledEvents();
        //    TunnelAllEvents();
        //}

        //void SubscribeToBubbledEvents()
        //{
        //    // private void __Dynamox_EVENTNAME_EventSubscription(arg1, arg2, etc...)
        //    var method = TypeBuilder.DefineMethod("__Dynamox_IEventChain_EventBubble_EventSubscription",
        //        MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.NewSlot | MethodAttributes.Final,
        //        null, new[] { typeof(EventChainArgs) });

        //    var body = method.GetILGenerator();

        //    foreach (var @event in
        //        TypeBuilder.BaseType.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        //        .Where(b => b.AddMethod != null && !b.AddMethod.IsPrivate && !b.AddMethod.IsAssembly))
        //    {
        //        var parameterTypes = @event.EventHandlerType.GetMethod("Invoke").GetParameters().Select(p => p.ParameterType).ToArray();
        //    // var eventArgs = new []{ arg_1, arg_2... };
        //    var eventArgs = body.CreateArray(typeof(object), parameterTypes.Length);
        //    for (var i = 0; i < parameterTypes.Length; i++)
        //    {
        //        body.Emit(OpCodes.Ldloc, eventArgs);
        //        body.Emit(OpCodes.Ldc_I4, i);
        //        body.Emit(OpCodes.Ldarg, i + 1);
        //        if (parameterTypes[i].IsValueType)
        //            body.Emit(OpCodes.Box, parameterTypes[i]);

        //        body.Emit(OpCodes.Stelem_Ref);
        //    }

        //    // var input = new EventChainArgs(this, "EVENTNAME", eventArgs);
        //    var input = body.DeclareLocal(typeof(EventChainArgs));
        //    body.Emit(OpCodes.Ldarg_0);
        //    body.Emit(OpCodes.Ldstr, @event.Name);
        //    body.Emit(OpCodes.Ldloc, eventArgs);
        //    body.Emit(OpCodes.Newobj, EventChainArgs);
        //    body.Emit(OpCodes.Stloc, input);

        //    // this.ObjectBase.EventTunnel(input);
        //    body.Emit(OpCodes.Ldarg_0);
        //    body.Emit(OpCodes.Ldfld, ObjBase);
        //    body.Emit(OpCodes.Ldloc, input);
        //    body.Emit(OpCodes.Callvirt, EventTunnel);


        //    }

        //    body.Emit(OpCodes.Ret);

        //    // this.EVENTNAME += __Dynamox_EVENTNAME_EventSubscription;
        //    MethodBody.Emit(OpCodes.Ldarg_0);
        //    MethodBody.Emit(OpCodes.Ldarg_0);
        //    MethodBody.Emit(OpCodes.Ldftn, method);
        //    MethodBody.Emit(OpCodes.Newobj, @event.EventHandlerType.GetConstructor(new[] { typeof(object), typeof(IntPtr) }));
        //    MethodBody.Emit(OpCodes.Callvirt, @event.AddMethod);

        //    ///*static readonly*/
        //    //MethodInfo Add_EventRaised = typeof(ObjectBase).GetEvent("EventRaised").AddMethod;

        //    //// var asIRaiseEvent = this as IRaiseEvent;
        //    //var asIRaiseEvent = MethodBody.DeclareLocal(typeof(IRaiseEvent));
        //    //MethodBody.Emit(OpCodes.Ldarg_0);
        //    //MethodBody.Emit(OpCodes.Castclass, typeof(IRaiseEvent));
        //    //MethodBody.Emit(OpCodes.Stloc, asIRaiseEvent);

        //    //// arg0.EventRaised += asIRaiseEvent.RaiseEvent;
        //    //MethodBody.Emit(OpCodes.Ldarg_1);
        //    //MethodBody.Emit(OpCodes.Ldloc, asIRaiseEvent);
        //    //MethodBody.Emit(OpCodes.Ldftn, RaiseEvent);
        //    //MethodBody.Emit(OpCodes.Newobj, _RaiseEventHandler);
        //    //MethodBody.Emit(OpCodes.Callvirt, Add_EventRaised);
        //}

        //static readonly ConstructorInfo EventChainArgs = typeof(EventChainArgs).GetConstructor(new[] { typeof(object), typeof(string), typeof(IEnumerable<object>) });
        //static readonly MethodInfo EventTunnel = typeof(IEventChain).GetMethod("EventTunnel");
        //void TunnelAllEvents()
        //{
        //    foreach (var @event in
        //        TypeBuilder.BaseType.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        //        .Where(b => b.AddMethod != null && !b.AddMethod.IsPrivate && !b.AddMethod.IsAssembly))
        //    {
        //        var parameterTypes = @event.EventHandlerType.GetMethod("Invoke").GetParameters().Select(p => p.ParameterType).ToArray();

        //        // private void __Dynamox_EVENTNAME_EventSubscription(arg1, arg2, etc...)
        //        var method = TypeBuilder.DefineMethod("__Dynamox_" + @event.Name + "_EventSubscription",
        //            MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.NewSlot | MethodAttributes.Final,
        //            null, parameterTypes);

        //        var body = method.GetILGenerator();

        //        // var eventArgs = new []{ arg_1, arg_2... };
        //        var eventArgs = body.CreateArray(typeof(object), parameterTypes.Length);
        //        for (var i = 0; i < parameterTypes.Length; i++)
        //        {
        //            body.Emit(OpCodes.Ldloc, eventArgs);
        //            body.Emit(OpCodes.Ldc_I4, i);
        //            body.Emit(OpCodes.Ldarg, i + 1);
        //            if (parameterTypes[i].IsValueType)
        //                body.Emit(OpCodes.Box, parameterTypes[i]);

        //            body.Emit(OpCodes.Stelem_Ref);
        //        }

        //        // var input = new EventChainArgs(this, "EVENTNAME", eventArgs);
        //        var input = body.DeclareLocal(typeof(EventChainArgs));
        //        body.Emit(OpCodes.Ldarg_0);
        //        body.Emit(OpCodes.Ldstr, @event.Name);
        //        body.Emit(OpCodes.Ldloc, eventArgs);
        //        body.Emit(OpCodes.Newobj, EventChainArgs);
        //        body.Emit(OpCodes.Stloc, input);

        //        // this.ObjectBase.EventTunnel(input);
        //        body.Emit(OpCodes.Ldarg_0);
        //        body.Emit(OpCodes.Ldfld, ObjBase);
        //        body.Emit(OpCodes.Ldloc, input);
        //        body.Emit(OpCodes.Callvirt, EventTunnel);

        //        body.Emit(OpCodes.Ret);

        //        // this.EVENTNAME += __Dynamox_EVENTNAME_EventSubscription;
        //        MethodBody.Emit(OpCodes.Ldarg_0);
        //        MethodBody.Emit(OpCodes.Ldarg_0);
        //        MethodBody.Emit(OpCodes.Ldftn, method);
        //        MethodBody.Emit(OpCodes.Newobj, @event.EventHandlerType.GetConstructor(new[] { typeof(object), typeof(IntPtr) }));
        //        MethodBody.Emit(OpCodes.Callvirt, @event.AddMethod);
        //    }
        //}

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