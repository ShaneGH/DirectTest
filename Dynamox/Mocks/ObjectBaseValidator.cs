using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Compile;

namespace Dynamox.Mocks
{
    /// <summary>
    /// Validate an object base against the type it will be injected into to attempt to discover any user errors
    /// </summary>
    internal class ObjectBaseValidator
    {
        public readonly TypeOverrideDescriptor ForType;

        public ObjectBaseValidator(TypeOverrideDescriptor forType)
        {
            ForType = forType;
        }

        public ObjectBaseValidator(Type forType)
            : this(TypeOverrideDescriptor.Create(forType))
        {
        }

        //TODO: indexes

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toValidate"></param>
        /// <returns>Errors</returns>
        public IEnumerable<string> ValidateAgainstType(ObjectBase toValidate)
        {
            var errors = new List<Error>();
            foreach (var item in toValidate.Members.Keys)
            {
                if (toValidate.Members[item] is MethodGroup)
                {
                    errors.Add(ValidateMethod(item, toValidate.Members[item] as MethodGroup, ForType.OverridableMethods));
                }
                else
                {
                    errors.Add(ValidateFieldOrProperty(item, toValidate.Members[item], ForType.SettableFields,
                        ForType.OverridableProperties.Union(ForType.SettableProperties)));
                }
            }

            return errors.Where(e => e != null).Select(e => e.ErrorMessage);
        }

        static Error ValidateMethod(string name, MethodGroup value, IEnumerable<MethodInfo> methods)
        {
            methods = methods.Where(m => m.Name == name).ToArray();
            if (!value.All(mock => methods.Any(m => mock.RepresentsMethod(m))))
                return new Error(Errors.CannotFindMethodToOverride, "Cannot find method \"" + name + "\" with the given parameters to mock.");

            return null;
        }

        static Error ValidateFieldOrProperty(string name, object value, IEnumerable<FieldInfo> fields, IEnumerable<PropertyInfo> properties)
        {
            properties = properties.Where(p => p.Name == name).ToArray();
            if (properties.Any())
            {
                if (value == null)
                {
                    if (!properties.Any(p => !p.PropertyType.IsValueType))
                        return new Error(Errors.PropertyTypeIsIncorrect, "The property \"" + name + "\" has type " + properties.First().PropertyType.Name + " which cannot be set to null.");
                    else
                        return null;
                }
                else if (properties.Any())
                {
                    if (properties.Any(p => p.PropertyType.IsAssignableFrom(value.GetType())))
                        return null;
                }
            }

            fields = fields.Where(p => p.Name == name).ToArray();
            if (fields.Any())
            {
                if (value == null)
                {
                    if (!fields.Any(p => !p.FieldType.IsValueType))
                        return new Error(Errors.PropertyTypeIsIncorrect, "The field \"" + name + "\" has type " + fields.First().FieldType.Name + " which cannot be set to null.");
                    else
                        return null;
                }
                else if (fields.Any())
                {
                    if (fields.Any(p => p.FieldType.IsAssignableFrom(value.GetType())))
                        return null;
                }
            }

            return new Error(Errors.CannotFindPropertyOrFieldToOverride, "Cannot find a field or property \"" + name + "\" to mock.");
        }

        class Error
        {
            public readonly Errors ErrorType;
            public readonly string ErrorMessage;

            public Error(Errors errorType, string errorMessage)
            {
                ErrorType = errorType;
                ErrorMessage = errorMessage;
            }
        }

        enum Errors
        {
            CannotFindMethodToOverride,
            CannotFindPropertyOrFieldToOverride,
            PropertyTypeIsIncorrect,
            FieldTypeIsIncorrect
        }
    }
}
