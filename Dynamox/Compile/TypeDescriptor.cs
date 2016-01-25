using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Compile
{
    /// <summary>
    /// Reflect over an interface to get the parts needed to build a virtual proxy
    /// </summary>
    internal class InterfaceDescriptor
    {
        public readonly Type Interface;

        public InterfaceDescriptor(Type @interface)
        {
            if (!@interface.IsInterface)
                throw new ArgumentException();  //TODO

            Interface = @interface;
        }

        IEnumerable<EventInfo> _OverridableEvents;
        public IEnumerable<EventInfo> OverridableEvents
        {
            get
            {
                if (_OverridableEvents == null)
                {
                    _OverridableEvents = Array.AsReadOnly(Interface.GetEvents().ToArray());
                }

                return _OverridableEvents;
            }
        }

        IEnumerable<MethodInfo> _AllEventAccessors;
        public IEnumerable<MethodInfo> AllEventAccessors
        {
            get
            {
                if (_AllEventAccessors == null)
                {
                    _AllEventAccessors = Array.AsReadOnly(Interface.GetEvents()
                        .SelectMany(e => new[] { e.AddMethod, e.RaiseMethod, e.RemoveMethod }.Union(e.GetOtherMethods(true)))
                        .Where(m => m != null)
                        .Distinct()
                        .ToArray());
                }

                return _AllEventAccessors;
            }
        }

        IEnumerable<PropertyInfo> _OverridableProperties;
        public IEnumerable<PropertyInfo> OverridableProperties
        {
            get
            {
                return _OverridableProperties ?? (_OverridableProperties =
                    Array.AsReadOnly(Interface.GetProperties().ToArray()));
            }
        }

        IEnumerable<MethodInfo> _AllPropertyAccessors;
        public IEnumerable<MethodInfo> AllPropertyAccessors
        {
            get
            {
                if (_AllPropertyAccessors == null)
                {
                    _AllPropertyAccessors = Array.AsReadOnly(Interface.GetProperties()
                        .SelectMany(p => p.GetAccessors(true))
                        .Distinct()
                        .ToArray());
                }

                return _AllPropertyAccessors;
            }
        }

        IEnumerable<MethodInfo> _OverridableMethods;
        public IEnumerable<MethodInfo> OverridableMethods
        {
            get
            {
                return _OverridableMethods ?? (_OverridableMethods =
                    Array.AsReadOnly(Interface.GetMethods().Where(m => !AllPropertyAccessors.Contains(m) && !AllEventAccessors.Contains(m)).ToArray()));
            }
        }

        public bool HasAbstractInternal
        {
            get { return false; }
        }
    }

    /// <summary>
    /// Reflect over a class to get the parts needed to build a virtual proxy
    /// </summary>
    internal class TypeOverrideDescriptor
    {
        public static readonly BindingFlags AllInstanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly Type _Type;
        public Type Type 
        {
            get
            {
                return _Type.IsInterface ? typeof(object) : _Type;
            }
        }

        static readonly ConcurrentDictionary<Type, TypeOverrideDescriptor> Cache;
        static TypeOverrideDescriptor()
        {
            Cache = new ConcurrentDictionary<Type, TypeOverrideDescriptor>();
            DxSettings.GlobalSettings.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "CacheTypeCheckers" && !DxSettings.GlobalSettings.CacheTypeCheckers)
                    Cache.Clear();
            };
        }

        public static TypeOverrideDescriptor Create(Type forType)
        {
            if (!DxSettings.GlobalSettings.CacheTypeCheckers)
                return new TypeOverrideDescriptor(forType);

            TypeOverrideDescriptor value;
            if (!Cache.TryGetValue(forType, out value))
                Cache.AddOrUpdate(forType,
                    value = new TypeOverrideDescriptor(forType),
                    (a, b) => value);

            return value;
        }

        private TypeOverrideDescriptor(Type type)
        {
            _Type = type;
        }
        
        bool? _HasAbstractInternal;
        public bool HasAbstractInternal
        {
            get
            {
                return _HasAbstractInternal ??
                    (_HasAbstractInternal = Type.GetMethods(AllInstanceMembers)
                        .Any(a => a.IsAbstract && a.IsAssembly && !a.IsFamilyOrAssembly)).Value;
            }
        }

        IEnumerable<EventInfo> _OverridableEvents;
        public IEnumerable<EventInfo> OverridableEvents
        {
            get
            {
                if (_OverridableEvents == null)
                {
                    _OverridableEvents = Array.AsReadOnly(
                        Type.GetEvents(AllInstanceMembers).Where(e => (e.IsAbstract() || e.IsVirtual()) && !e.IsFinal() && !e.IsAssembly() && !e.IsPrivate())
                        .ToArray());
                }

                return _OverridableEvents;
            }
        }

        IEnumerable<EventInfo> _AllEvents;
        public IEnumerable<EventInfo> AllEvents
        {
            get
            {
                if (_AllEvents == null)
                {
                    _AllEvents = Array.AsReadOnly(InheritanceTree
                        .SelectMany(e => e.GetEvents(AllInstanceMembers))
                        .Concat(OverridableInterfaces.SelectMany(i => i.OverridableEvents))
                        .Distinct()
                        .ToArray());
                }

                return _AllEvents;
            }
        }

        IEnumerable<MethodInfo> _AllEventAccessors;
        public IEnumerable<MethodInfo> AllEventAccessors
        {
            get
            {
                if (_AllEventAccessors == null)
                {
                    _AllEventAccessors = Array.AsReadOnly(AllEvents
                        .SelectMany(e => new[] { e.AddMethod, e.RaiseMethod, e.RemoveMethod }.Union(e.GetOtherMethods(true)))
                        .Where(m => m != null)
                        .Distinct()
                        .ToArray());
                }

                return _AllEventAccessors;
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
                        (_Type.IsInterface ? new[] { _Type } : Type.EmptyTypes).Union(_Type.GetInterfaces())
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
                    _OverridableProperties = Array.AsReadOnly(Type.GetProperties(AllInstanceMembers)
                        .Where(p => (p.IsAbstract() || p.IsVirtual()) && !p.IsFinal() && !p.IsAssembly() && !p.IsPrivate())
                        .ToArray());
                }

                return _OverridableProperties;
            }
        }

        IEnumerable<FieldInfo> _SettableFields;
        public IEnumerable<FieldInfo> SettableFields
        {
            get
            {
                if (_SettableFields == null)
                {
                    _SettableFields = Array.AsReadOnly(Type.GetFields(AllInstanceMembers)
                        .Where(f => !f.IsPrivate && !f.IsAssembly && !f.IsInitOnly)
                        .ToArray());
                }

                return _SettableFields;
            }
        }

        IEnumerable<PropertyInfo> _SettableProperties;
        public IEnumerable<PropertyInfo> SettableProperties
        {
            get
            {
                if (_SettableProperties == null)
                {
                    _SettableProperties = Array.AsReadOnly(Type.GetProperties(AllInstanceMembers)
                        .Where(p => !OverridableProperties.Contains(p))
                        .Where(p => p.SetMethod != null && !p.SetMethod.IsPrivate && !p.SetMethod.IsAssembly && p.GetMethod != null)
                        .ToArray());
                }

                return _SettableProperties;
            }
        }

        IEnumerable<MethodInfo> _AllPropertyAccessors;
        public IEnumerable<MethodInfo> AllPropertyAccessors
        {
            get
            {
                if (_AllPropertyAccessors == null)
                {
                    _AllPropertyAccessors = Array.AsReadOnly(InheritanceTree
                        .SelectMany(p => p.GetProperties(AllInstanceMembers))
                        .SelectMany(p => p.GetAccessors(true))
                        .Distinct()
                        .ToArray());
                }

                return _AllPropertyAccessors;
            }
        }

        IEnumerable<MethodInfo> _OverridableMethods;
        public IEnumerable<MethodInfo> OverridableMethods
        {
            get
            {
                if (_OverridableMethods == null)
                {
                    _OverridableMethods = Array.AsReadOnly(AllMethods
                        .Where(m => (m.IsAbstract || m.IsVirtual) && !m.IsFinal)
                        .Where(m => !m.IsAssembly && !m.IsPrivate && !AllPropertyAccessors.Contains(m) && !AllEventAccessors.Contains(m))
                        .Where(m => m.Name != "Finalize" || m.ReturnType != typeof(void) || m.GetParameters().Length != 0)
                        .ToArray());
                }

                return _OverridableMethods;
            }
        }

        IEnumerable<MethodInfo> _AllMethods;
        public IEnumerable<MethodInfo> AllMethods
        {
            get
            {
                if (_AllMethods == null)
                {
                    _AllMethods = Array.AsReadOnly(Type.GetMethods(AllInstanceMembers));
                }

                return _AllMethods;
            }
        }

        public bool MethodClashes(MethodInfo method)
        {
            return AllMethods.Any(m => MethodSignatureComparer.Instance.Equals(method, m) && m != method);
        }

        IEnumerable<Type> _InheritanceTree;
        IEnumerable<Type> InheritanceTree
        {
            get
            {
                if (_InheritanceTree == null)
                {
                    var type = Type;
                    var output = new List<Type>();
                    while (type != null)
                    {
                        output.Insert(0, type);
                        type = type.BaseType;
                    }

                    _InheritanceTree = output.AsReadOnly();
                }

                return _InheritanceTree;
            }
        }
    }
}
