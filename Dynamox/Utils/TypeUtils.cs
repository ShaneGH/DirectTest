using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class TypeUtils
    {
        public static IEnumerable<Type> AllClassesAndInterfaces(this Type type)
        {
            var output = new List<Type>();
            while (type != null)
            {
                output.Add(type);
                output.AddRange(type.GetInterfaces());
                type = type.BaseType;
            }

            return output.Where(o => o != null).Distinct();
        }

        public static bool IsVirtual(this PropertyInfo property)
        {
            return property.GetAccessors(true).Any(a => a.IsVirtual);
        }

        public static bool IsAbstract(this PropertyInfo property)
        {
            return property.GetAccessors(true).Any(a => a.IsAbstract);
        }

        public static bool IsPrivate(this PropertyInfo property)
        {
            return property.GetAccessors(true).All(a => a.IsPrivate);
        }

        public static bool IsFinal(this PropertyInfo property)
        {
            return property.GetAccessors(true).Any(a => a.IsFinal);
        }

        public static bool IsAssembly(this PropertyInfo property)
        {
            return property.GetAccessors(true).All(a => a.IsAssembly);
        }

        //TODO: test
        public static bool IsAbstract(this EventInfo @event)
        {
            return CheckEventFlag(@event, e => e.IsAbstract);
        }

        //TODO: test
        public static bool IsVirtual(this EventInfo @event)
        {
            return CheckEventFlag(@event, e => e.IsVirtual);
        }

        //TODO: test
        public static bool IsPrivate(this EventInfo @event)
        {
            return CheckEventFlag(@event, e => e.IsPrivate);
        }

        //TODO: test
        public static bool IsAssembly(this EventInfo @event)
        {
            return CheckEventFlag(@event, e => e.IsAssembly);
        }

        //TODO: test
        public static bool IsFinal(this EventInfo @event)
        {
            return CheckEventFlag(@event, e => e.IsFinal);
        }

        private static bool CheckEventFlag(EventInfo @event, Func<MethodInfo, bool> flag)
        {
            //return flag(@event.AddMethod) || flag(@event.RemoveMethod) || flag(@event.RaiseMethod);


            return (@event.AddMethod != null && flag(@event.AddMethod)) ||
                (@event.RemoveMethod != null && flag(@event.RemoveMethod)) ||
                (@event.RaiseMethod != null && flag(@event.RaiseMethod));
        }

        //These functions are not correct
        //public static bool IsPublic(this PropertyInfo property)
        //{
        //    return property.GetAccessors().Any(a => a.IsPublic);
        //}

        //public static bool IsProtected(this PropertyInfo property)
        //{
        //    return property.GetAccessors().Any(a => a.IsAssembly) &&
        //        property.GetAccessors().All(a => !a.IsPublic && !a.IsFamily && !a.IsFamilyOrAssembly);
        //}

        //public static bool IsInternal(this PropertyInfo property)
        //{
        //    return property.GetAccessors().Any(a => a.IsAssembly) &&
        //        property.GetAccessors().All(a => !a.IsPublic && !a.IsFamily && !a.IsFamilyOrAssembly);
        //}

        //public static bool IsPrivate(this PropertyInfo property)
        //{
        //    return property.GetAccessors().All(a => a.IsPrivate);
        //}
    }
}
