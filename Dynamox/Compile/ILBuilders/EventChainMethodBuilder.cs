using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Compile.ILBuilders
{
    /// <summary>
    /// Implement the IRaiseEvent interface
    /// </summary>
    public class EventChainMethodBuilder : NewBlockILBuilder
    {
        //static readonly EventInfo EventBubble = typeof(IEventChain).GetEvent("EventBubble");
        //static readonly MethodInfo EventTunnel = typeof(IEventChain).GetMethod("EventTunnel");

        public EventChainMethodBuilder(TypeBuilder toType, FieldInfo objBase)
            : base(toType, objBase)
        {
        }

        protected override void _Build()
        {
            //// no need to add twice or if there are no events on the object
            //if (TypeBuilder.GetInterfaces().Contains(typeof(IEventChain)))
            //    return;

            //if (!TypeBuilder.GetInterfaces().Contains(typeof(IRaiseEvent)))
            //    throw new InvalidOperationException();  //TODE

            //TypeBuilder.AddInterfaceImplementation(typeof(IEventChain));

            //AddEventBubble();
            //AddEventTunnel();
        }

        //void AddEventBubble()
        //{
        //    new EventBuilder(TypeBuilder, ObjBase, EventBubble).Build();
        //}

        //void AddEventTunnel()
        //{
        //    var method = TypeBuilder.DefineMethod("IEventChain.EventTunnel",
        //        MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Private | MethodAttributes.NewSlot | MethodAttributes.Final,
        //        null, new[] { typeof(EventChainArgs) });

        //    var body = method.GetILGenerator();

        //    body.Emit(OpCodes.Ldarg_0);
        //    body.Emit(OpCodes.Ldfld, ObjBase);
        //    body.Emit(OpCodes.Ldarg_1);
        //    body.Emit(OpCodes.Callvirt, EventTunnel);

        //    // add any more prperties/fields which should be tunneled to here

        //    body.Emit(OpCodes.Ret);

        //    TypeBuilder.DefineMethodOverride(method, EventTunnel);
        //}
    }
}