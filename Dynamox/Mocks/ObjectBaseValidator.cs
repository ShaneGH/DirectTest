//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using Dynamox.Compile;

//namespace Dynamox.Mocks
//{
//    /// <summary>
//    /// Validate an object base against the type it will be injected into to attempt to discover any user errors
//    /// </summary>
//    public class ObjectBaseValidator
//    {
//        public readonly TypeOverrideDescriptor ForType;

//        public ObjectBaseValidator(TypeOverrideDescriptor forType)
//        {
//            ForType = forType;
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="toValidate"></param>
//        /// <returns>Errors</returns>
//        public IEnumerable<string> ValidateAgainstType(ObjectBase toValidate)
//        {
//            Errors? error;
//            var errors = new List<Errors?>();
//            foreach (var item in toValidate.Members.Keys)
//            {
//                if (toValidate.Members[item] is MethodGroup)
//                    errors.Add(ValidateMethod(item, toValidate.Members[item] as MethodGroup, ForType.OverridableMethods));
//                else
//                {
//                    error = ValidateFieldOrProperty(item, toValidate.Members[item], ForType.OverridableFields, ForType.OverridableProperties);
//                    if (error == Errors.CannotFindPropertyOrFieldToOverride)
//                    {
//                        if ()
//                    }
//                    else
//                    {
//                        errors.Add(error);
//                    }
//                }
//            }

//            return errors.Where(e => e.HasValue).Select(e => e.Value);
//        }
            
//        // TODO: return more complex structure to near misses (partially matched functions)
//        static Error ValidateMethod(string name, MethodGroup value, IEnumerable<MethodInfo> methods)
//        {
//            methods = methods.Where(m => m.Name == name).ToArray();
//            if (!value.All(mock => methods.Any(m => mock.RepresentsMethod(m))))
//                return Errors.CannotFindMethodToOverride;

//            return null;
//        }

//        // TODO: return more complex structure to give field or property types
//        static Error ValidateFieldOrProperty(string name, object value, IEnumerable<FieldInfo> fields, IEnumerable<PropertyInfo> properties)
//        {
//            properties = properties.Where(p => p.Name == name).ToArray();
//            if (value == null)
//            {
//                if (!properties.Any(p => !p.PropertyType.IsValueType))
//                    return Errors.PropertyTypeIsIncorrect;
//            }
//            else
//            {
//                return null;
//            }

//            if (properties.Any(p => p.PropertyType.IsAssignableFrom(value.GetType())))
//                return null;

//            fields = fields.Where(p => p.Name == name).ToArray();
//            if (value == null)
//            {
//                if (!fields.Any(p => !p.FieldType.IsValueType))
//                    return Errors.FieldTypeIsIncorrect;
//            }
//            else
//            {
//                return null;
//            }

//            if (fields.Any(p => p.FieldType.IsAssignableFrom(value.GetType())))
//                return null;

//            return new Error(Errors.CannotFindPropertyOrFieldToOverride, "Cannot find a field or property \"" + name + "\" to mock.");
//        }

//        class Error
//        {
//            readonly Errors ErrorType;
//            readonly string ErrorMessage;

//            public Error(Errors errorType, string errorMessage)
//            {
//                ErrorType = errorType;
//                ErrorMessage = errorMessage;
//            }
//        }

//        enum Errors
//        {
//            CannotFindMethodToOverride,
//            CannotFindPropertyOrFieldToOverride,
//            PropertyTypeIsIncorrect,
//            FieldTypeIsIncorrect
//        }
//    }
//}
