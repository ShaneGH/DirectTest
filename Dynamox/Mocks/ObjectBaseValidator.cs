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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toValidate"></param>
        /// <returns>Errors</returns>
        public IEnumerable<string> ValidateAgainstType(ObjectBase toValidate)
        {
            var properties = ForType.OverridableProperties.Union(ForType.SettableProperties)
                .Where(p => !p.GetIndexParameters().Any()).ToArray();
            var indexes = ForType.OverridableProperties.Union(ForType.SettableProperties)
                .Where(p => p.GetIndexParameters().Any()).ToArray();

            var errors = new List<Error>();
            foreach (var item in toValidate.Members.Keys)
            {
                if (toValidate.Members[item] is MethodGroup)
                {
                    errors.Add(ValidateMethod(item, toValidate.Members[item] as MethodGroup, ForType.OverridableMethods));
                }
                else
                {
                    errors.Add(ValidateFieldOrProperty(item, toValidate.Members[item], ForType.SettableFields, properties));
                }
            }

            errors.AddRange(toValidate.MockedIndexes.Select(i => ValidateIndex(indexes, i.Key.Select(k => k == null ? null : k.GetType()), i.Value == null ? null : i.Value.GetType())));
            return errors.Where(e => e != null).Select(e => e.ErrorMessage);
        }

        static Error ValidateIndex(IEnumerable<PropertyInfo> indexedProperties, IEnumerable<Type> keyTypes, Type value)
        {
            Func<Type, Type, bool> validateType = (a, b) =>
            {
                if (b == null)
                {
                    if (a.IsValueType)
                        return false;
                }
                else if (a == null)
                {
                    if (b.IsValueType)
                        return false;
                }
                else if (!b.IsAssignableFrom(a))
                {
                    return false;
                }

                return true;
            };

            foreach (var i in indexedProperties.Select(ip => new 
                { value = ip.PropertyType, key = ip.GetIndexParameters().Select(p => p.ParameterType).ToArray() }))
            {
                if (!validateType(value, i.value))
                    continue;

                if (keyTypes.Count() != i.key.Length)
                    continue;

                int current = 0;
                foreach (var key in keyTypes)
                {
                    if (!validateType(key, i.key[current]))
                    {
                        current = -1;
                        break;
                    }

                    current++;
                }

                if (current != -1)
                    return null;
            }

            return new Error(Errors.PropertyTypeIsIncorrect,
                "Cannot find an indexed property with indexes: " +
                string.Join(", ", keyTypes.Select(k => k == null ? "null" : k.ToString()).ToArray()) +
                " and value " + value == null ? "null" : value.ToString());
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
