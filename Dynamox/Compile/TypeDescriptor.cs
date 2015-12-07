using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Compile
{
    internal interface ITypeOverrideDescriptor
    {
        IEnumerable<PropertyInfo> OverridableProperties { get; }
        IEnumerable<MethodInfo> OverridablePropertyAccessors { get; }
        IEnumerable<MethodInfo> OverridableMethods { get; }
        bool HasAbstractInternal { get; }
    }

    internal class InterfaceDescriptor : ITypeOverrideDescriptor
    {
        public readonly Type Interface;

        public InterfaceDescriptor(Type _interface)
        {
            if (!_interface.IsInterface)
                throw new ArgumentException();  //TODO

            Interface = _interface;
        }

        IEnumerable<PropertyInfo> _OverridableProperties;
        public IEnumerable<PropertyInfo> OverridableProperties
        {
            get
            {
                return _OverridableProperties ?? ( _OverridableProperties = Array.AsReadOnly(Interface.GetProperties()));
            }
        }

        IEnumerable<MethodInfo> _OverridablePropertyAccessors;
        public IEnumerable<MethodInfo> OverridablePropertyAccessors
        {
            get 
            {
                return _OverridablePropertyAccessors ?? (_OverridablePropertyAccessors =
                    Array.AsReadOnly(OverridableProperties.SelectMany(p => p.GetAccessors(true)).ToArray()));
            }
        }

        IEnumerable<MethodInfo> _OverridableMethods;
        public IEnumerable<MethodInfo> OverridableMethods
        {
            get
            {
                return _OverridableMethods ?? (_OverridableMethods = 
                    Array.AsReadOnly(Interface.GetMethods().Where(m => !OverridablePropertyAccessors.Contains(m)).ToArray()));
            }
        }

        public bool HasAbstractInternal
        {
            get { return false; }
        }
    }

    internal class TypeOverrideDescriptor : ITypeOverrideDescriptor
    {
        public static readonly BindingFlags AllInstanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public readonly Type Type;

        public TypeOverrideDescriptor(Type type)
        {
            Type = type;
        }
        
        bool? _HasAbstractInternal;
        public bool HasAbstractInternal
        {
            get
            {
                return _HasAbstractInternal ?? 
                    (_HasAbstractInternal = OverridablePropertyAccessors.Any(a => a.IsAbstract && a.IsAssembly && !a.IsFamilyOrAssembly) ||
                    OverridableMethods.Any(a => a.IsAbstract && a.IsAssembly && !a.IsFamilyOrAssembly)).Value;
            }
        }

        IEnumerable<InterfaceDescriptor> _OverridableInterfaces;
        public IEnumerable<InterfaceDescriptor> OverridableInterfaces
        {
            get
            {
                if (_OverridableInterfaces == null)
                {
                    _OverridableInterfaces = Array.AsReadOnly(
                        (Type.IsInterface ? new[] { Type } : new Type[0]).Union(Type.GetInterfaces())
                        .Select(i => new InterfaceDescriptor(i))
                        .ToArray());
                }

                return _OverridableInterfaces;
            }
        }

        IEnumerable<PropertyInfo> _OverridableProperties;
        public IEnumerable<PropertyInfo> OverridableProperties
        {
            get
            {
                if (_OverridableProperties == null)
                {
                    //_OverridableProperties = Type.GetProperties(AllInstanceMembers)
                    //    .Where(p => !p.IsPrivate())
                    _OverridableProperties = InheritanceTree
                        .SelectMany(p => p.GetProperties(AllInstanceMembers))
                        // PropertyName*System.Int32
                        .GroupBy(t => t.Name + "*" + t.PropertyType)
                        .Select(m => Youngest(m))
                        .Where(p => (p.IsAbstract() || p.IsVirtual()) && !p.IsFinal())
                        .ToList()
                        .AsReadOnly();
                }

                return _OverridableProperties;
            }
        }

        IEnumerable<MethodInfo> _OverridablePropertyAccessors;
        public IEnumerable<MethodInfo> OverridablePropertyAccessors
        {
            get
            {
                if (_OverridablePropertyAccessors == null)
                {
                    _OverridablePropertyAccessors = 
                        OverridableProperties.SelectMany(p => p.GetAccessors())
                            .ToList()
                            .AsReadOnly();
                }

                return _OverridablePropertyAccessors;
            }
        }

        IEnumerable<MethodInfo> _OverridableMethods;
        public IEnumerable<MethodInfo> OverridableMethods
        {
            get
            {
                if (_OverridableMethods == null)
                {
                    //_OverridableMethods = InheritanceTree
                    //    .SelectMany(m => m.GetMethods(AllInstanceMembers))
                    _OverridableMethods = Type.GetMethods(AllInstanceMembers)
                        .Where(m => !m.IsPrivate && !OverridablePropertyAccessors.Contains(m))
                        // MethodName*System.Void*System.Int32*System.Int32=5*System.String=$null$
                        //.GroupBy(t => t.Name + "*" +
                        //    t.ReturnType.AssemblyQualifiedName +
                        //    t.GetParameters().Select(p => "*" + p.ParameterType.AssemblyQualifiedName +
                        //        (p.HasDefaultValue ? ("=" + p.DefaultValue == null ? "$null$" : p.DefaultValue.GetHashCode().ToString()) : "")))
                        //.Select(m => Youngest(m))
                        .Where(m => (m.IsAbstract || m.IsVirtual) && !m.IsFinal)
                        .Where(m => m.Name != "Finalize" || m.ReturnType != typeof(void) || m.GetParameters().Length != 0)
                        .ToList()
                        .AsReadOnly();
                }

                return _OverridableMethods;
            }
        }

        /// <summary>
        /// For a group of methods which are all the same, return the one which has been defeind latest in the inheritance tree
        /// </summary>
        /// <param name="methods"></param>
        /// <returns></returns>
        static MethodInfo Youngest(IEnumerable<MethodInfo> methods)
        {
            var output = methods.FirstOrDefault();
            foreach (var method in methods.Skip(1))
            {
                if (!MethodInfoComparer.Instance.Equals(output, method))
                    throw new InvalidOperationException("Methods are not in the same inheritance tree");    //TODO

                if (output.DeclaringType.IsAssignableFrom(method.DeclaringType))
                {
                    output = method;
                }
                else if (!method.DeclaringType.IsAssignableFrom(output.DeclaringType))
                {
                    throw new InvalidOperationException("Methods are not in the same inheritance tree");    //TODO
                }
            }

            return output;
        }

        /// <summary>
        /// For a group of methods which are all the same, return the one which has been defeind latest in the inheritance tree
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        static PropertyInfo Youngest(IEnumerable<PropertyInfo> properties)
        {
            var output = properties.FirstOrDefault();
            foreach (var property in properties.Skip(1))
            {
                if (output.Name != property.Name || output.PropertyType != property.PropertyType)
                    throw new InvalidOperationException("Properties are not in the same inheritance tree");    //TODO

                if (output.DeclaringType.IsAssignableFrom(property.DeclaringType))
                {
                    output = property;
                }
                else if (!property.DeclaringType.IsAssignableFrom(output.DeclaringType))
                {
                    throw new InvalidOperationException("Properties are not in the same inheritance tree");    //TODO
                }
            }

            return output;
        }

        IEnumerable<Type> InheritanceTree
        {
            get
            {
                if (Type == null || Type.IsInterface)
                    return Enumerable.Empty<Type>();

                var type = Type;
                var output = new List<Type>();
                while (type != null)
                {
                    output.Insert(0, type);
                    type = type.BaseType;
                }

                return output.AsReadOnly();
            }
        }
    }
}
