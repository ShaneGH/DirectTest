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
