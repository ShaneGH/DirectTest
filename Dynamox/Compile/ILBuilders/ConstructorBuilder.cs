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
        readonly bool IsIEventChain;
        readonly ConstructorInfo Constructor;
        readonly TypeOverrideDescriptor Descriptor;
        ILGenerator MethodBody { get; set; }

        IEnumerable<EventInfo> Events
        {
            get
            {
                // there will be duplicate events if inheritance/interfaces are used
                return Descriptor.AllEvents
                    .GroupBy(e => new Tuple<string, Type>(e.Name, e.EventHandlerType))
                    .Select(g => g.Count() < 2 ? g : g.Take(1))
                    .SelectMany(g => g);
            }
        }

        public ConstructorBuilder(TypeBuilder toType, FieldInfo objBase, ConstructorInfo constructor, TypeOverrideDescriptor descriptor, bool isIEventChain)
            : base(toType, objBase)
        {
            Constructor = constructor;
            Descriptor = descriptor;
            IsIEventChain = isIEventChain;
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

            if (IsIEventChain)
                AddIEventParasiteFunctionality();

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

        void AddIEventParasiteFunctionality()
        {
            BuildRaiseEventsFromParasiteFunctionality();
            BuildReportEventsToParasiteFunctionality();
        }

        static readonly MethodInfo EventHandlerFound = typeof(EventShareEventArgs).GetProperty("EventHandlerFound").SetMethod;
        static readonly MethodInfo RaiseEvent = typeof(IRaiseEvent).GetMethod("RaiseEvent");
        static readonly MethodInfo ToArray = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(typeof(object));
        static readonly FieldInfo EventName = typeof(EventShareEventArgs).GetField("EventName");
        static readonly FieldInfo EventArgs = typeof(EventShareEventArgs).GetField("EventArgs");
        static readonly EventInfo RaiseEventCalled = typeof(IEventParasite).GetEvent("RaiseEventCalled");
        void BuildRaiseEventsFromParasiteFunctionality()
        {
            // private void __Dynamox_IEventParasite_RaiseEventCalled(EventShareEventArgs, args)
            var method = TypeBuilder.DefineMethod("__Dynamox_IEventParasite_RaiseEventCalled",
                MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.NewSlot | MethodAttributes.Final,
                null, new[] { typeof(EventShareEventArgs) });

            var body = method.GetILGenerator();

            var _return = body.DefineLabel();

            // if (args == null) return;
            body.CheckForNull(OpCodes.Ldarg_1, ifNull: _return);

            // var eventArgs = args.EventArgs.ToArray();
            var eventArgs = body.DeclareLocal(typeof(object[]));
            body.Emit(OpCodes.Ldarg_1);
            body.Emit(OpCodes.Ldfld, EventArgs);
            body.Emit(OpCodes.Call, ToArray);
            body.Emit(OpCodes.Stloc, eventArgs);

            // var result = RaiseEvent(args.EventName, eventArgs);
            var result = body.DeclareLocal(typeof(bool));
            body.Emit(OpCodes.Ldarg_0);
            body.Emit(OpCodes.Ldarg_1);
            body.Emit(OpCodes.Ldfld, EventName);
            body.Emit(OpCodes.Ldloc, eventArgs);
            body.Emit(OpCodes.Callvirt, RaiseEvent);
            body.Emit(OpCodes.Stloc, result);

            // args.EventHandlerFound = result
            body.Emit(OpCodes.Ldarg_1);
            body.Emit(OpCodes.Ldloc, result);
            body.Emit(OpCodes.Call, EventHandlerFound);

            // return
            body.MarkLabel(_return);
            body.Emit(OpCodes.Ret);

            // BACK TO CONSTRUCTOR
            // arg0.RaiseEventCalled += __Dynamox_IEventParasite_RaiseEventCalled;
            MethodBody.Emit(OpCodes.Ldarg_1);
            MethodBody.Emit(OpCodes.Ldarg_0);
            MethodBody.Emit(OpCodes.Ldftn, method);
            MethodBody.Emit(OpCodes.Newobj, RaiseEventCalled.EventHandlerType.GetConstructor(new[] { typeof(object), typeof(IntPtr) }));
            MethodBody.Emit(OpCodes.Callvirt, RaiseEventCalled.AddMethod);
        }

        static readonly ConstructorInfo EventChainArgs = typeof(EventShareEventArgs).GetConstructor(new[] { typeof(string), typeof(IEnumerable<object>) });
        static readonly MethodInfo EventRaised = typeof(ObjectBase).GetMethod("EventRaised");
        void BuildReportEventsToParasiteFunctionality()
        {
            foreach (var @event in Events)
            {
                var parameterTypes = @event.EventHandlerType.GetMethod("Invoke").GetParameters().Select(p => p.ParameterType).ToArray();

                // private void __Dynamox_EVENTNAME_EventSubscription(arg1, arg2, etc...)
                var method = TypeBuilder.DefineMethod("__Dynamox_" + @event.Name + "_EventSubscription",
                    MethodAttributes.HideBySig | MethodAttributes.Private | MethodAttributes.NewSlot | MethodAttributes.Final,
                    null, parameterTypes);

                var body = method.GetILGenerator();

                // var eventArgs = new []{ arg_1, arg_2... };
                var eventArgs = body.CreateArray(typeof(object), parameterTypes.Length);
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    body.Emit(OpCodes.Ldloc, eventArgs);
                    body.Emit(OpCodes.Ldc_I4, i);
                    body.Emit(OpCodes.Ldarg, i + 1);
                    if (parameterTypes[i].IsValueType)
                        body.Emit(OpCodes.Box, parameterTypes[i]);

                    body.Emit(OpCodes.Stelem_Ref);
                }

                // var input = new EventChainArgs("EVENTNAME", eventArgs);
                var input = body.DeclareLocal(typeof(EventShareEventArgs));
                body.Emit(OpCodes.Ldstr, @event.Name);
                body.Emit(OpCodes.Ldloc, eventArgs);
                body.Emit(OpCodes.Newobj, EventChainArgs);
                body.Emit(OpCodes.Stloc, input);

                // this.ObjectBase.EventRaised(input);
                body.Emit(OpCodes.Ldarg_0);
                body.Emit(OpCodes.Ldfld, ObjBase);
                body.Emit(OpCodes.Ldloc, input);
                body.Emit(OpCodes.Callvirt, EventRaised);

                body.Emit(OpCodes.Ret);

                // BACK TO CONSTRUCTOR
                // this.EVENTNAME += __Dynamox_EVENTNAME_EventSubscription;
                MethodBody.Emit(OpCodes.Ldarg_0);
                MethodBody.Emit(OpCodes.Ldarg_0);
                MethodBody.Emit(OpCodes.Ldftn, method);
                MethodBody.Emit(OpCodes.Newobj, @event.EventHandlerType.GetConstructor(new[] { typeof(object), typeof(IntPtr) }));
                MethodBody.Emit(OpCodes.Callvirt, @event.AddMethod);
            }
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