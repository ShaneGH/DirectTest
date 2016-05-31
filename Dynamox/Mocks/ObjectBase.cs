using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Compile;
using Dynamox.Dynamic;
using Dynamox.Mocks.Info;

namespace Dynamox.Mocks
{
    /// <summary>
    /// The underlying functionality behind a proxy class 
    /// </summary>
    public class ObjectBase : IEventParasite, IEnsure
    {
        public static readonly Meta Reflection = new Meta();

        readonly bool StrictMock;
        public readonly DxSettings Settings;
        readonly MockBuilder MockedInfo;
        readonly Dictionary<string, object> ExtraAddedProperties = new Dictionary<string, object>();
        readonly List<IndexedProperty> ExtraAddedIndexes = new List<IndexedProperty>();

        public ReadOnlyDictionary<string, object> Members 
        {
            get
            {
                return MockedInfo.Values;
            }
        }

        public IEnumerable<IndexedProperty> MockedIndexes
        {
            get
            {
                return MockedInfo.IndexedValues;
            }
        }

        IEnumerable<IndexedProperty> Indexes
        {
            get
            {
                return ExtraAddedIndexes.Concat(MockedIndexes);
            }
        }

        IEnumerable<KeyValuePair<string, object>> Properties
        {
            get
            {
                return ExtraAddedProperties.Concat(
                    Members.Where(m => !(m.Value is MethodGroup)));
            }
        }

        IEnumerable<KeyValuePair<string, MethodGroup>> Methods
        {
            get
            {
                return Members
                    .Where(m => m.Value is MethodGroup)
                    .Select(m => new KeyValuePair<string, MethodGroup>(m.Key, m.Value as MethodGroup));
            }
        }

        public ObjectBase(DxSettings settings, bool strictMock = false)
            : this(settings, new MockBuilder(settings), strictMock)
        {
        }

        internal ObjectBase(DxSettings settings, MockBuilder mockedInfo, bool strictMock = false)
        {
            Settings = settings;
            StrictMock = strictMock;
            MockedInfo = mockedInfo;

            // pass through
            MockedInfo.RaiseEventCalled += (args) => 
            {
                if (RaiseEventCalled != null)
                    RaiseEventCalled(args);
            };
        }

        static TValue ConvertAndReturn<TValue>(object input, string methodOrPropertyDescription)
        {
            if (input is MockBuilder)
                return (TValue)(input as MockBuilder).Mock(typeof(TValue));
            else if (input is IPropertyMockAccessor)
                return (input as IPropertyMockAccessor).Get<TValue>();
            else if ((input == null && !typeof(TValue).IsValueType) || input is TValue)
                return (TValue)input;
            else
                throw new InvalidMockException("Cannot return type " + (input ?? "null") + " from " +
                    methodOrPropertyDescription + ". Expected " + typeof(TValue) + ".");
        }

        public static bool HasEmptyConstructor(Type t)
        {
            return GetEmptyConstructor(t) != null;
        }

        public static ConstructorInfo GetEmptyConstructor(Type t)
        {
            var constructors = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            return constructors.FirstOrDefault(c => !c.GetParameters().Any());
        }

        /// <summary>
        /// Determine if an object is the desired type or can be converted (not cast) to the desired type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toTest"></param>
        /// <returns></returns>
        private bool Is<T>(object toTest)
        {
            return (toTest == null && !typeof(T).IsValueType) ||
                toTest is T ||
                toTest is IPropertyMockBuilder<T> ||
                toTest is MockBuilder;
        }

        #region Properties

        public bool HasMockedFieldOrProperty<TProperty>(string name)
        {
            return Members.ContainsKey(name) && Is<TProperty>(Members[name]);
        }

        public void SetProperty(string propertyName, object propertyValue)
        {
            _SetProperty(propertyName, propertyValue, true);
        }

        public bool TrySetProperty(string propertyName, object propertyValue)
        {
            return _SetProperty(propertyName, propertyValue, false);
        }

        bool _SetProperty(string propertyName, object propertyValue, bool forceSet)
        {
            if (!forceSet && (!Members.ContainsKey(propertyName) || Members[propertyName] is MethodGroup))
                return false;

            lock (ExtraAddedProperties)
            {
                if (Members.ContainsKey(propertyName) && Members[propertyName] is IPropertyMockAccessor)
                {
                    (Members[propertyName] as IPropertyMockAccessor).Set(propertyValue);

                    if (ExtraAddedProperties.ContainsKey(propertyName))
                        ExtraAddedProperties.Remove(propertyName);
                }
                else if (ExtraAddedProperties.ContainsKey(propertyName))
                {
                    ExtraAddedProperties[propertyName] = propertyValue;
                }
                else
                {
                    ExtraAddedProperties.Add(propertyName, propertyValue);
                }
            }

            return true;
        }

        public TProperty GetProperty<TProperty>(string propertyName, bool recordAccess)
        {
            TProperty result;
            if (!TryGetProperty(propertyName, recordAccess, out result))
                result = default(TProperty);

            return result;
        }

        public bool TryGetProperty<TProperty>(string propertyName, bool recordAccess, out TProperty result)
        {
            KeyValuePair<string, object> property;
            lock (ExtraAddedProperties)
            {
                property = Properties.FirstOrDefault(p => p.Key == propertyName);
            }

            if (property.Equals(default(KeyValuePair<string, object>)))
            {
                if (StrictMock)
                {
                    throw new InvalidMockException("Property " + propertyName + " of type " + typeof(TProperty) + " has not been mocked");
                }
                else
                {
                    result = default(TProperty);
                    return false;
                }
            }

            result = ConvertAndReturn<TProperty>(property.Value, "property " + propertyName);
            if (recordAccess) MockedInfo.AccessedProperty(propertyName);
            return true;
        }

        #endregion

        #region Indexed Properties

        public IEnumerable<IEnumerable<MethodArg>> GetMockedIndexKeys<TProperty>(IEnumerable<Type> keys)
        {
            var ks = keys.ToArray();
            return MockedIndexes.Where(m => m.Keys.Count() == keys.Count() && Is<TProperty>(m.Value))
                .Select(m => new
                {
                    result = m.Keys.Select((k, i) =>
                        (k == null && !ks[i].IsValueType) ||
                        (k != null && ks[i].IsAssignableFrom(k.GetType()))
                    ).All(x => x),
                    keys = m.Keys
                })
                .Where(m => m.result)
                .Select(m => m.keys.Select((k, i) => new MethodArg(k, ks[i], i.ToString())).ToArray())
                .ToArray();
        }

        public TIndexed GetIndex<TIndexed>(IEnumerable<MethodArg> indexValues, bool recordAccess)
        {
            TIndexed result;
            if (!TryGetIndex(indexValues, recordAccess, out result))
                result = default(TIndexed);

            return result;
        }

        public void SetIndex(IEnumerable<MethodArg> indexValues, object value)
        {
            _SetIndex(indexValues, value, true);
        }

        public bool TrySetIndex(IEnumerable<MethodArg> indexValues, object value)
        {
            return _SetIndex(indexValues, value, true);
        }

        bool _SetIndex(IEnumerable<MethodArg> indexValues, object value, bool forceSet)
        {
            var keys = indexValues.Select(v => v.Arg).ToArray();

            var existing = MockedIndexes.FirstOrDefault(i => i.CompareKeys(keys, false));
            if (!forceSet && existing != null)
                return false;

            lock (ExtraAddedIndexes)
            {
                if (existing != null && existing.Value is IPropertyMockAccessor)
                {
                    (existing.Value as IPropertyMockAccessor).Set(value);
                    return true;
                }

                ExtraAddedIndexes.Insert(0, new IndexedProperty(keys, value));
                return true;
            }
        }

        public bool TryGetIndex<TIndexed>(IEnumerable<MethodArg> indexValues, bool recordAccess, out TIndexed result)
        {
            IndexedProperty value;
            var keys = indexValues.Select(v => v.Arg).ToArray();
            lock (ExtraAddedProperties)
            {
                value = Indexes.FirstOrDefault(i => i.CompareKeys(keys));
            }

            if (value == null)
            {
                if (StrictMock)
                {
                    throw new InvalidMockException("Indexed property for values " + string.Join(", ", indexValues.Select(v => v.ArgType)) +
                        " of type " + typeof(TIndexed) + " has not been mocked");
                }
                else
                {
                    result = default(TIndexed);
                    return false;
                }
            }

            result = ConvertAndReturn<TIndexed>(value.Value, "index");
            if (recordAccess) MockedInfo.AccessedIndexedProperty(indexValues.Select(i => i.Arg));
            return true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke a method with no return value
        /// </summary>
        public void Invoke(string methodName, IEnumerable<MethodArg> arguments)
        {
            Invoke(methodName, Enumerable.Empty<Type>(), arguments);
        }

        /// <summary>
        /// Invoke a method with no return value
        /// </summary>
        public void Invoke(string methodName, IEnumerable<Type> genericArguments, IEnumerable<MethodArg> arguments)
        {
            TryInvoke(methodName, genericArguments, arguments);
        }

        /// <summary>
        /// Invoke a method with a return value
        /// </summary>
        public TResult Invoke<TResult>(string methodName, IEnumerable<MethodArg> arguments)
        {
            return Invoke<TResult>(methodName, Enumerable.Empty<Type>(), arguments);
        }

        /// <summary>
        /// Invoke a method with a return value
        /// </summary>
        public TResult Invoke<TResult>(string methodName, IEnumerable<Type> genericArguments, IEnumerable<MethodArg> arguments)
        {
            TResult result;
            TryInvoke(methodName, genericArguments, arguments, out result);
            return result;
        }

        /// <summary>
        /// Invoke a method with no return value
        /// </summary>
        public bool TryInvoke(string methodName, IEnumerable<MethodArg> arguments)
        {
            return TryInvoke(methodName, Enumerable.Empty<Type>(), arguments);
        }

        /// <summary>
        /// Just a floag to say the method has no retunr value
        /// </summary>
        private sealed class MethodVoid { private MethodVoid() { } }

        /// <summary>
        /// Invoke a method with no return value
        /// </summary>
        public bool TryInvoke(string methodName, IEnumerable<Type> genericArguments, IEnumerable<MethodArg> arguments)
        {
            MethodVoid dummy = null;
            return TryInvoke<MethodVoid>(methodName, genericArguments, arguments, out dummy);
        }

        /// <summary>
        /// Invoke a method with a return value
        /// </summary>
        public bool TryInvoke<TResult>(string methodName, IEnumerable<MethodArg> arguments, out TResult result)
        {
            return TryInvoke<TResult>(methodName, Enumerable.Empty<Type>(), arguments, out result);
        }

        /// <summary>
        /// Invoke a method with a return value
        /// </summary>
        public bool TryInvoke<TResult>(string methodName, IEnumerable<Type> genericArguments, IEnumerable<MethodArg> arguments, out TResult result)
        {
            object tmp = null;
            var method = Methods
                .Where(m => m.Key == methodName)
                .FirstOrDefault(m => m.Value.TryInvoke(genericArguments, arguments, out tmp));

            if (method.Equals(default(KeyValuePair<string, MethodGroup>)))
            {
                if (StrictMock)
                {
                    var args = arguments.Any() ? " with arguments " + string.Join(", ", arguments.Select(a => a.ArgType.Name)) : string.Empty;
                    var genericParams = genericArguments.Any() ? ((arguments.Any() ? " and" : " with") + " generic arguments " + 
                        string.Join(", ", genericArguments.Select(a => a.Name))) : string.Empty;
                    throw new InvalidMockException("Method " + methodName + args + genericParams + " has not been mocked");
                }
                else
                {
                    result = default(TResult);
                    return false;
                }
            }

            if (typeof(TResult) == typeof(MethodVoid))
            {
                result = default(TResult);
                return true;
            }

            if (tmp is MockBuilder)
                tmp = (tmp as MockBuilder).Mock(typeof(TResult));

            if (tmp is TResult || (tmp == null && !typeof(TResult).IsValueType))
            {
                result = (TResult)tmp;
                return true;
            }

            if (tmp == null)
            {
                result = default(TResult);
                return false;
            }

            throw new InvalidMockException("Cannot return type " + tmp + " from method " + 
                methodName + ". Expected " + typeof(TResult) + ".");
        }

        #endregion

        #region events

        public event EventShareHandler RaiseEventCalled;

        public void EventRaised(EventShareEventArgs args)
        {
            // pass through
            MockedInfo.EventRaised(args);
        }

        #endregion

        #region Meta

        public class Meta
        {
            public readonly MethodInfo Invoke = 
                TypeUtils.GetMethod<ObjectBase>(a => a.Invoke(default(string), default(IEnumerable<MethodArg>)));
            public readonly MethodInfo InvokeReturnValue =
                TypeUtils.GetMethod<ObjectBase>(a => a.Invoke<string>(default(string), default(IEnumerable<MethodArg>)));
            public readonly MethodInfo InvokeGeneric =
                TypeUtils.GetMethod<ObjectBase>(a => a.Invoke(default(string), default(IEnumerable<Type>), default(IEnumerable<MethodArg>)));
            public readonly MethodInfo InvokeGenericReturnValue =
                TypeUtils.GetMethod<ObjectBase>(a => a.Invoke<string>(default(string), default(IEnumerable<Type>), default(IEnumerable<MethodArg>)));

            static string _dummy;
            public readonly MethodInfo TryInvoke =
                TypeUtils.GetMethod<ObjectBase>(a => a.TryInvoke(default(string), default(IEnumerable<MethodArg>)));
            public readonly MethodInfo TryInvokeReturnValue =
                TypeUtils.GetMethod<ObjectBase>(a => a.TryInvoke<string>(default(string), default(IEnumerable<MethodArg>), out _dummy));
            public readonly MethodInfo TryInvokeGeneric =
                TypeUtils.GetMethod<ObjectBase>(a => a.TryInvoke(default(string), default(IEnumerable<Type>), default(IEnumerable<MethodArg>)));
            public readonly MethodInfo TryInvokeGenericReturnValue =
                TypeUtils.GetMethod<ObjectBase>(a => a.TryInvoke<string>(default(string), default(IEnumerable<Type>), default(IEnumerable<MethodArg>), out _dummy));

            public readonly MethodInfo TryGetProperty =
                TypeUtils.GetMethod<ObjectBase>(a => a.TryGetProperty<string>(default(string), default(bool), out _dummy));
            public readonly MethodInfo GetProperty =
                TypeUtils.GetMethod<ObjectBase>(a => a.GetProperty<string>(default(string), default(bool)));
            public readonly MethodInfo TrySetProperty =
                TypeUtils.GetMethod<ObjectBase>(a => a.TrySetProperty(default(string), default(object)));
            public readonly MethodInfo SetProperty =
                TypeUtils.GetMethod<ObjectBase>(a => a.SetProperty(default(string), default(object)));

            public readonly MethodInfo TryGetIndex =
                TypeUtils.GetMethod<ObjectBase>(a => a.TryGetIndex<string>(default(IEnumerable<MethodArg>), default(bool), out _dummy));
            public readonly MethodInfo GetIndex =
                TypeUtils.GetMethod<ObjectBase>(a => a.GetIndex<string>(default(IEnumerable<MethodArg>), default(bool)));
            public readonly MethodInfo TrySetIndex =
                TypeUtils.GetMethod<ObjectBase>(a => a.TrySetIndex(default(IEnumerable<MethodArg>), default(object)));
            public readonly MethodInfo SetIndex =
                TypeUtils.GetMethod<ObjectBase>(a => a.SetIndex(default(IEnumerable<MethodArg>), default(object)));
            public readonly MethodInfo GetMockedIndexKeys =
                TypeUtils.GetMethod<ObjectBase>(a => a.GetMockedIndexKeys<object>(default(IEnumerable<Type>)));

            public readonly MethodInfo HasMockedFieldOrProperty =
                TypeUtils.GetMethod<ObjectBase>(a => a.HasMockedFieldOrProperty<string>(default(string)));

            public readonly FieldInfo Settings = TypeUtils.GetField<ObjectBase, DxSettings>(a => a.Settings);
        }

        #endregion

        #region Ensure

        public IEnumerable<string> ShouldHaveBeenCalled
        {
            get { return MockedInfo.ShouldHaveBeenCalled; }
        }

        #endregion
    }
}
