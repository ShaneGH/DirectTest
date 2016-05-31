using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dynamox.Compile.ILBuilders
{
    /// <summary>
    /// Build an event for the given type, based on an event from an ancestor.
    /// If the ancestor is an interface and the type already has an event of the same name, the event will not be built.
    /// If built, the EventField property is set
    /// </summary>
    public class EventBuilder : NewBlockILBuilder
    {
        static string _dummy;
        static readonly ConcurrentDictionary<Type, MethodInfo> CompareExchange = new ConcurrentDictionary<Type, MethodInfo>();
        private static readonly MethodInfo _CompareExchange = 
            TypeUtils.GetMethod(() => Interlocked.CompareExchange<string>(ref _dummy, default(string), default(string)));

        readonly EventInfo ParentEvent;
        public FieldInfo EventField { get; private set; }

        public EventBuilder(TypeBuilder toType, FieldInfo objBase, EventInfo parentEvent)
            : base(toType, objBase)
        {
            ParentEvent = parentEvent;
            if (!CompareExchange.ContainsKey(ParentEvent.EventHandlerType))
                CompareExchange.TryAdd(ParentEvent.EventHandlerType, _CompareExchange.MakeGenericMethod(ParentEvent.EventHandlerType));
        }

        protected override void _Build()
        {
            // only add interface events if it is not hiding another event
            if (ParentEvent.DeclaringType.IsInterface && 
                TypeBuilder.BaseType.GetEvents(Compiler.AllMembers).Any(e => e.Name == ParentEvent.Name))
                return;

            if (!ParentEvent.IsAbstract() && !ParentEvent.IsVirtual())
                throw new CompilerException(TypeBuilder.BaseType, "Cannot mock non virtual event " + ParentEvent.Name);

            EventField = TypeBuilder.DefineField(ParentEvent.Name, ParentEvent.EventHandlerType, FieldAttributes.Private);
            var @event = TypeBuilder.DefineEvent(ParentEvent.Name, EventAttributes.None, ParentEvent.EventHandlerType);

            @event.SetAddOnMethod(BuildAddMethod());
            @event.SetRemoveOnMethod(BuildRemoveMethod());
        }

        MethodAttributes GetMethodAttributes(MethodInfo method)
        {
            var attrs = MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
            attrs = attrs | (ParentEvent.DeclaringType.IsInterface ? MethodAttributes.NewSlot : MethodAttributes.ReuseSlot);
            var attr = MethodBuilder.GetAccessAttr(method);
            if (attr.HasValue)
                attrs = attrs | attr.Value;

            return attrs;
        }

        static readonly MethodInfo DelegateCombine = TypeUtils.GetMethod(() => Delegate.Combine(default(Delegate), default(Delegate)));

        System.Reflection.Emit.MethodBuilder BuildAddMethod()
        {
            //        .method public hidebysig newslot specialname virtual 
            //        instance void  add_Event(class [mscorlib]System.EventHandler 'value') cil managed
            var addEvent = TypeBuilder.DefineMethod("add_" + ParentEvent.Name, 
                GetMethodAttributes(ParentEvent.AddMethod), null, new[] { ParentEvent.EventHandlerType });
            var body = addEvent.GetILGenerator();

            //{
            //  // Code size       48 (0x30)
            //  .maxstack  3
            //  .locals init (class [mscorlib]System.EventHandler V_0,
            //           class [mscorlib]System.EventHandler V_1,
            //           class [mscorlib]System.EventHandler V_2,
            //           bool V_3)
            var var0 = body.DeclareLocal(ParentEvent.EventHandlerType);
            var var1 = body.DeclareLocal(ParentEvent.EventHandlerType);
            var var2 = body.DeclareLocal(ParentEvent.EventHandlerType);
            var var3 = body.DeclareLocal(typeof(bool));

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

            body.Emit(OpCodes.Call, DelegateCombine);
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
            body.Emit(OpCodes.Call, CompareExchange[ParentEvent.EventHandlerType]);
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

            return addEvent;
        }

        static readonly MethodInfo DelegateRemove = TypeUtils.GetMethod(() => Delegate.Remove(default(Delegate), default(Delegate)));

        System.Reflection.Emit.MethodBuilder BuildRemoveMethod()
        {
            //            .method public hidebysig newslot specialname virtual 
            //        instance void  remove_Event(class [mscorlib]System.EventHandler 'value') cil managed
            var removeEvent = TypeBuilder.DefineMethod("remove_" + ParentEvent.Name, 
                GetMethodAttributes(ParentEvent.RemoveMethod), null, new[] { ParentEvent.EventHandlerType });
            var body = removeEvent.GetILGenerator();

            //{
            //  // Code size       48 (0x30)
            //  .maxstack  3
            //  .locals init (class [mscorlib]System.EventHandler V_0,
            //           class [mscorlib]System.EventHandler V_1,
            //           class [mscorlib]System.EventHandler V_2,
            //           bool V_3)
            var var0 = body.DeclareLocal(ParentEvent.EventHandlerType);
            var var1 = body.DeclareLocal(ParentEvent.EventHandlerType);
            var var2 = body.DeclareLocal(ParentEvent.EventHandlerType);
            var var3 = body.DeclareLocal(typeof(bool));

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
            body.Emit(OpCodes.Call, DelegateRemove);
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
            body.Emit(OpCodes.Call, CompareExchange[ParentEvent.EventHandlerType]);
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

            return removeEvent;
        }
    }
}
