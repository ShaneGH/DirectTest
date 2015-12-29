using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Compile
{
    public class EventBuilder : IlBuilder
    {
        private static readonly MethodInfo _CompareExchange = typeof(System.Threading.Interlocked)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .First(m => m.Name == "CompareExchange" && m.GetGenericArguments().Any());

        readonly EventInfo ParentEvent;
        public FieldInfo EventField { get; private set; }
        readonly MethodInfo CompareExchange;

        public EventBuilder(TypeBuilder toType, FieldInfo objBase, EventInfo parentEvent)
            : base(toType, objBase)
        {
            ParentEvent = parentEvent;
            CompareExchange = _CompareExchange.MakeGenericMethod(ParentEvent.EventHandlerType);
        }

        /// <summary>
        /// only add interface events if it is not hiding another event
        /// </summary>
        /// <param name="toType"></param>
        /// <param name="parentEvent"></param>
        /// <returns></returns>
        public static bool ShouldAddInterfaceEvent(Type toType, EventInfo parentEvent)
        {
            return !toType.GetEvents(Compiler.AllMembers).Any(e => e.Name == parentEvent.Name);
        }

        protected override void _Build()
        {
            if (ParentEvent.DeclaringType.IsInterface && !ShouldAddInterfaceEvent(ToType.BaseType, ParentEvent))
                return;

            if (!ParentEvent.IsAbstract() && !ParentEvent.IsVirtual())
                throw new InvalidOperationException();  //TODE

            EventField = ToType.DefineField(ParentEvent.Name, ParentEvent.EventHandlerType, FieldAttributes.Private);
            var @event = ToType.DefineEvent(ParentEvent.Name, EventAttributes.None, ParentEvent.EventHandlerType);

            @event.SetAddOnMethod(BuildAddEvent());
            @event.SetRemoveOnMethod(BuildRemoveEvent());
        }

        System.Reflection.Emit.MethodBuilder BuildAddEvent()
        {
            var attrs = MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
            attrs = attrs | (ParentEvent.DeclaringType.IsInterface ? MethodAttributes.NewSlot : MethodAttributes.ReuseSlot);
            var attr = MethodBuilder.GetAccessAttr(ParentEvent.AddMethod);
            if (attr.HasValue)
                attrs = attrs | attr.Value;

            //        .method public hidebysig newslot specialname virtual 
            //        instance void  add_Event(class [mscorlib]System.EventHandler 'value') cil managed
            var add_Event = ToType.DefineMethod("add_" + ParentEvent.Name, attrs, null, new[] { ParentEvent.EventHandlerType });
            var body = add_Event.GetILGenerator();

            var var0 = body.DeclareLocal(ParentEvent.EventHandlerType);
            var var1 = body.DeclareLocal(ParentEvent.EventHandlerType);
            var var2 = body.DeclareLocal(ParentEvent.EventHandlerType);
            var var3 = body.DeclareLocal(typeof(bool));
            //{
            //  // Code size       48 (0x30)
            //  .maxstack  3
            //  .locals init (class [mscorlib]System.EventHandler V_0,
            //           class [mscorlib]System.EventHandler V_1,
            //           class [mscorlib]System.EventHandler V_2,
            //           bool V_3)
            //  IL_0000:  ldarg.0
            body.Emit(OpCodes.Ldarg_0);
            //  IL_0001:  ldfld      class [mscorlib]System.EventHandler Dynamox.Tests.C1::Event
            body.Emit(OpCodes.Ldfld, EventField);
            //  IL_0006:  stloc.0
            body.Emit(OpCodes.Stloc, var0);

            var beginAgain = body.DefineLabel();
            body.MarkLabel(beginAgain);

            //  IL_0007:  ldloc.0
            body.Emit(OpCodes.Ldloc, var0);
            //  IL_0008:  stloc.1
            body.Emit(OpCodes.Stloc, var1);
            //  IL_0009:  ldloc.1
            body.Emit(OpCodes.Ldloc, var1);
            //  IL_000a:  ldarg.1
            body.Emit(OpCodes.Ldarg_1);
            //  IL_000b:  call       class [mscorlib]System.Delegate [mscorlib]System.Delegate::Combine(class [mscorlib]System.Delegate,
            //                                                                                          class [mscorlib]System.Delegate)

            body.Emit(OpCodes.Call, typeof(Delegate).GetMethod("Combine", new[] { typeof(Delegate), typeof(Delegate) }));
            //  IL_0010:  castclass  [mscorlib]System.EventHandler
            body.Emit(OpCodes.Castclass, ParentEvent.EventHandlerType);
            //  IL_0015:  stloc.2
            body.Emit(OpCodes.Stloc, var2);
            //  IL_0016:  ldarg.0
            body.Emit(OpCodes.Ldarg_0);
            //  IL_0017:  ldflda     class [mscorlib]System.EventHandler Dynamox.Tests.C1::Event
            body.Emit(OpCodes.Ldflda, EventField);
            //  IL_001c:  ldloc.2
            body.Emit(OpCodes.Ldloc, var2);
            //  IL_001d:  ldloc.1
            body.Emit(OpCodes.Ldloc, var1);
            //  IL_001e:  call       !!0 [mscorlib]System.Threading.Interlocked::CompareExchange<class [mscorlib]System.EventHandler>(!!0&,
            //                                                                                                                        !!0,
            //                                                                                                                        !!0)
            body.Emit(OpCodes.Call, CompareExchange);
            //  IL_0023:  stloc.0
            body.Emit(OpCodes.Stloc, var0);
            //  IL_0024:  ldloc.0
            body.Emit(OpCodes.Ldloc, var0);
            //  IL_0025:  ldloc.1
            body.Emit(OpCodes.Ldloc, var1);
            //  IL_0026:  ceq
            body.Emit(OpCodes.Ceq);
            //  IL_0028:  ldc.i4.0
            body.Emit(OpCodes.Ldc_I4_0);
            //  IL_0029:  ceq
            body.Emit(OpCodes.Ceq);
            //  IL_002b:  stloc.3
            body.Emit(OpCodes.Stloc, var3);
            //  IL_002c:  ldloc.3
            body.Emit(OpCodes.Ldloc, var3);
            //  IL_002d:  brtrue.s   IL_0007
            body.Emit(OpCodes.Brtrue_S, beginAgain);
            //  IL_002f:  ret
            body.Emit(OpCodes.Ret);
            ////} // end of method C1::add_Event

            return add_Event;
        }

        System.Reflection.Emit.MethodBuilder BuildRemoveEvent()
        {
            var attrs = MethodAttributes.HideBySig | MethodAttributes.Virtual;
            attrs = attrs | (ParentEvent.DeclaringType.IsInterface ? MethodAttributes.NewSlot : MethodAttributes.ReuseSlot);
            var attr = MethodBuilder.GetAccessAttr(ParentEvent.RemoveMethod);
            if (attr.HasValue)
                attrs = attrs | attr.Value;


            //            .method public hidebysig newslot specialname virtual 
            //        instance void  remove_Event(class [mscorlib]System.EventHandler 'value') cil managed
            var remove_Event = ToType.DefineMethod("remove_" + ParentEvent.Name, attrs, null, new[] { ParentEvent.EventHandlerType });
            var body = remove_Event.GetILGenerator();

            var var0 = body.DeclareLocal(ParentEvent.EventHandlerType);
            var var1 = body.DeclareLocal(ParentEvent.EventHandlerType);
            var var2 = body.DeclareLocal(ParentEvent.EventHandlerType);
            var var3 = body.DeclareLocal(typeof(bool));
            //{
            //  // Code size       48 (0x30)
            //  .maxstack  3
            //  .locals init (class [mscorlib]System.EventHandler V_0,
            //           class [mscorlib]System.EventHandler V_1,
            //           class [mscorlib]System.EventHandler V_2,
            //           bool V_3)
            //  IL_0000:  ldarg.0
            body.Emit(OpCodes.Ldarg_0);
            //  IL_0001:  ldfld      class [mscorlib]System.EventHandler Dynamox.Tests.C1::Event
            body.Emit(OpCodes.Ldfld, EventField);
            //  IL_0006:  stloc.0
            body.Emit(OpCodes.Stloc, var0);

            var beginAgain = body.DefineLabel();
            body.MarkLabel(beginAgain);
            //  IL_0007:  ldloc.0
            body.Emit(OpCodes.Ldloc, var0);
            //  IL_0008:  stloc.1
            body.Emit(OpCodes.Stloc, var1);
            //  IL_0009:  ldloc.1
            body.Emit(OpCodes.Ldloc, var1);
            //  IL_000a:  ldarg.1
            body.Emit(OpCodes.Ldarg_1);
            //  IL_000b:  call       class [mscorlib]System.Delegate [mscorlib]System.Delegate::Remove(class [mscorlib]System.Delegate,
            //                                                                                         class [mscorlib]System.Delegate)
            body.Emit(OpCodes.Call, typeof(Delegate).GetMethod("Remove", new[] { typeof(Delegate), typeof(Delegate) }));
            //  IL_0010:  castclass  [mscorlib]System.EventHandler
            body.Emit(OpCodes.Castclass, ParentEvent.EventHandlerType);
            //  IL_0015:  stloc.2
            body.Emit(OpCodes.Stloc, var2);
            //  IL_0016:  ldarg.0
            body.Emit(OpCodes.Ldarg_0);
            //  IL_0017:  ldflda     class [mscorlib]System.EventHandler Dynamox.Tests.C1::Event
            body.Emit(OpCodes.Ldflda, EventField);
            //  IL_001c:  ldloc.2
            body.Emit(OpCodes.Ldloc, var2);
            //  IL_001d:  ldloc.1
            body.Emit(OpCodes.Ldloc, var1);
            //  IL_001e:  call       !!0 [mscorlib]System.Threading.Interlocked::CompareExchange<class [mscorlib]System.EventHandler>(!!0&,
            //                                                                                                                        !!0,
            //                                                                                                                        !!0)
            body.Emit(OpCodes.Call, CompareExchange);
            //  IL_0023:  stloc.0
            body.Emit(OpCodes.Stloc, var0);
            //  IL_0024:  ldloc.0
            body.Emit(OpCodes.Ldloc, var0);
            //  IL_0025:  ldloc.1
            body.Emit(OpCodes.Ldloc, var1);
            //  IL_0026:  ceq
            body.Emit(OpCodes.Ceq);
            //  IL_0028:  ldc.i4.0
            body.Emit(OpCodes.Ldc_I4_0);
            //  IL_0029:  ceq
            body.Emit(OpCodes.Ceq);
            //  IL_002b:  stloc.3
            body.Emit(OpCodes.Stloc, var3);
            //  IL_002c:  ldloc.3
            body.Emit(OpCodes.Ldloc, var3);
            //  IL_002d:  brtrue.s   IL_0007
            body.Emit(OpCodes.Brtrue_S, beginAgain);
            //  IL_002f:  ret
            body.Emit(OpCodes.Ret);
            //} // end of method C1::remove_Event

            return remove_Event;
        }

        static readonly MethodInfo _CheckEventArgs = typeof(Compiler).GetMethod("CheckEventArgs", new[] { typeof(IEnumerable<object>), typeof(Type) });
        static readonly MethodInfo StringEquals = typeof(string).GetMethod("Equals", new[] { typeof(string) });
        public static void AddRaiseEventMethod(TypeBuilder toType, IEnumerable<FieldInfo> events)
        {
            toType.AddInterfaceImplementation(typeof(IRaiseEvent));

            var method = toType.DefineMethod("IRaiseEvent.RaiseEvent",
                MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Private | MethodAttributes.NewSlot | MethodAttributes.Final,
                typeof(bool), new[] { typeof(string), typeof(object[]) });

            var body = method.GetILGenerator();
            var @return = body.DefineLabel();

            var output = body.DeclareLocal(typeof(bool));
            body.Emit(OpCodes.Ldc_I4_0);
            body.Emit(OpCodes.Stloc, output);

            foreach (var eventField in events)
            {
                var invoke = eventField.FieldType.GetMethod("Invoke");
                var eventArgs = invoke.GetParameters().Select(p => p.ParameterType).ToArray();
                var eventHandlerType = body.DeclareLocal(typeof(Type));
                var next = body.DefineLabel();

                // var eventHandlerType = typeof(TheEventHandler);
                body.TypeOf(eventField.FieldType);
                body.Emit(OpCodes.Stloc, eventHandlerType);

                // if ("EventName".Equals(eventName)) GO TO Next
                body.Emit(OpCodes.Ldstr, eventField.Name);
                body.Emit(OpCodes.Ldarg_1);
                body.Emit(OpCodes.Call, StringEquals);
                body.Emit(OpCodes.Ldc_I4_0);
                body.Emit(OpCodes.Ceq);
                body.Emit(OpCodes.Brtrue, next);

                // output = true
                body.Emit(OpCodes.Ldc_I4_1);
                body.Emit(OpCodes.Stloc, output);

                // CheckEventArgs(arg1, eventHandlerType);
                body.Emit(OpCodes.Ldarg_2);
                body.Emit(OpCodes.Ldloc, eventHandlerType);
                body.Emit(OpCodes.Call, _CheckEventArgs);

                // if (EventName == null) GO TO Return;
                body.Emit(OpCodes.Ldarg_0);
                body.Emit(OpCodes.Ldfld, eventField);
                body.Emit(OpCodes.Ldnull);
                body.Emit(OpCodes.Ceq);
                body.Emit(OpCodes.Brtrue, @return);

                // EventName((T)arg1[0], (T)arg1[1]....);
                body.Emit(OpCodes.Ldarg_0);
                body.Emit(OpCodes.Ldfld, eventField);
                for (var i = 0; i < eventArgs.Length; i++)
                {
                    // args[i]
                    body.LoadArrayElement(OpCodes.Ldarg_2, i);

                    // (T)args[i]
                    if (eventArgs[i] == typeof(object)) ;
                    else if (eventArgs[i].IsValueType)
                    {
                        body.Emit(OpCodes.Unbox_Any, eventArgs[i]);
                    }
                    else
                    {
                        body.Emit(OpCodes.Castclass, eventArgs[i]);
                    }
                }

                body.Emit(OpCodes.Callvirt, invoke);

                body.Emit(OpCodes.Br, @return);
                body.MarkLabel(next);
            }

            body.MarkLabel(@return);

            // return output
            body.Emit(OpCodes.Ldloc, output);
            body.Emit(OpCodes.Ret);
            toType.DefineMethodOverride(method, typeof(IRaiseEvent).GetMethod("RaiseEvent"));
        }
    }
}
