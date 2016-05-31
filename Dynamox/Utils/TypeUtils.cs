using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class TypeUtils
    {
        static MethodInfo GetMethod(Expression expression, bool useConstructedGeneric = false)
        {
            // remove implicit casts
            while (expression is UnaryExpression)
                expression = (expression as UnaryExpression).Operand;

            var method = expression is MethodCallExpression ?
                (expression as MethodCallExpression).Method : null;

            return method != null && !useConstructedGeneric && method.IsGenericMethod ?
                method.GetGenericMethodDefinition() :
                method;
        }

        public static MethodInfo GetMethod<TType>(Expression<Action<TType>> expression, bool useConstructedGeneric = false)
        {
            return GetMethod(expression.Body, useConstructedGeneric);
        }

        public static MethodInfo GetMethod(Expression<Action> expression, bool useConstructedGeneric = false)
        {
            return GetMethod(expression.Body, useConstructedGeneric);
        }

        public static T GetFieldOrProperty<T>(Expression expression)
            where T  : MemberInfo
        {
            if (expression.NodeType == ExpressionType.ArrayLength)
            {
                return (expression as UnaryExpression).Operand.Type.MakeArrayType().GetProperty("Length") as T;
            }

            // remove implicit casts
            while (expression is UnaryExpression)
                expression = (expression as UnaryExpression).Operand;

            return expression is MemberExpression ?
                (expression as MemberExpression).Member as T : null;
        }

        public static PropertyInfo GetProperty<TType, TReturn>(Expression<Func<TType, TReturn>> expression)
        {
            return GetFieldOrProperty<PropertyInfo>(expression.Body);
        }

        public static PropertyInfo GetProperty<TReturn>(Expression<Func<TReturn>> expression)
        {
            return GetFieldOrProperty<PropertyInfo>(expression.Body);
        }

        public static FieldInfo GetField<TType, TReturn>(Expression<Func<TType, TReturn>> expression)
        {
            return GetFieldOrProperty<FieldInfo>(expression.Body);
        }

        public static FieldInfo GetField<TReturn>(Expression<Func<TReturn>> expression)
        {
            return GetFieldOrProperty<FieldInfo>(expression.Body);
        }

        public static FieldInfo GetField<TType>(Expression<Action<TType>> expression)
        {
            return GetFieldOrProperty<FieldInfo>(expression.Body);
        }

        public static FieldInfo GetField(Expression<Action> expression)
        {
            return GetFieldOrProperty<FieldInfo>(expression.Body);
        }

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
