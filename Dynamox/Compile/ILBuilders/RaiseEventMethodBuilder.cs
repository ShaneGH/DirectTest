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
    /// Implement the IRaiseEvent interface
    /// </summary>
    public class RaiseEventMethodBuilder : NewBlockILBuilder
    {
        static readonly MethodInfo RaiseEvent = typeof(IRaiseEvent).GetMethod("RaiseEvent");
        static readonly MethodInfo _CheckEventArgs = typeof(RaiseEventMethodBuilder).GetMethod("CheckEventArgs", new[] { typeof(IEnumerable<object>), typeof(Type) });
        static readonly MethodInfo StringEquals = typeof(string).GetMethod("Equals", new[] { typeof(string) });
        readonly IEnumerable<FieldInfo> Events;

        public RaiseEventMethodBuilder(TypeBuilder toType, FieldInfo objBase, IEnumerable<FieldInfo> events)
            : base(toType, objBase)
        {
            Events = Array.AsReadOnly(events.ToArray());
        }

        protected override void _Build()
        {
            // no need to add twice or if there are no events on the object
            if (TypeBuilder.GetInterfaces().Contains(typeof(IRaiseEvent)) || !Events.Any())
                return;

            TypeBuilder.AddInterfaceImplementation(typeof(IRaiseEvent));

            var method = TypeBuilder.DefineMethod("IRaiseEvent.RaiseEvent",
                MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Private | MethodAttributes.NewSlot | MethodAttributes.Final,
                typeof(bool), new[] { typeof(string), typeof(object[]) });

            var body = method.GetILGenerator();
            var @return = body.DefineLabel();

            var output = body.DeclareLocal(typeof(bool));
            body.Emit(OpCodes.Ldc_I4_0);
            body.Emit(OpCodes.Stloc, output);

            foreach (var eventField in Events)
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
            TypeBuilder.DefineMethodOverride(method, RaiseEvent);
        }

        public static void CheckEventArgs(IEnumerable<object> args, Type eventHandlerType)
        {
            var arguments = args.ToArray();
            var argTypes = eventHandlerType.GetMethod("Invoke").GetParameters().Select(i => i.ParameterType).ToArray();

            if (arguments.Length != argTypes.Length)
                ThrowException(eventHandlerType, arguments, argTypes);

            for (var i = 0; i < arguments.Length; i++)
            {
                if ((arguments[i] == null && argTypes[i].IsValueType) ||
                    (arguments[i] != null && !argTypes[i].IsAssignableFrom(arguments[i].GetType())))
                    ThrowException(eventHandlerType, arguments, argTypes);
            }
        }

        static void ThrowException(Type eventHandlerType, object[] arguments, Type[] expectedArguments)
        {
            throw new InvalidMockException("Invalid event args for event handler: " + eventHandlerType + "." + Environment.NewLine +
                "Expected: " + string.Join(", ", expectedArguments.Select(a => a.Name)) + Environment.NewLine +
                "Actual: " + string.Join(", ", arguments.Select(a => a == null ? "null" : a.GetType().Name)));
        }
    }
}
