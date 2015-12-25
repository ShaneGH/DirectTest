using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Dynamic;

namespace Dynamox.Mocks
{
    public class ObjectBase
    {
        public static readonly Meta Reflection = new Meta();

        readonly bool StrictMock;
        public readonly DxSettings Settings;
        public readonly ReadOnlyDictionary<string, object> Members;
        public readonly ReadOnlyDictionary<IEnumerable<object>, object> MockedIndexes;
        readonly Dictionary<string, object> ExtraAddedProperties = new Dictionary<string, object>();
        readonly Dictionary<IEnumerable<object>, object> ExtraAddedIndexes = new Dictionary<IEnumerable<object>, object>(DynamicBag.ArrayComparer);

        IEnumerable<KeyValuePair<IEnumerable<object>, object>> Indexes
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
            : this(settings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()), strictMock)
        {
        }

        public ObjectBase(DxSettings settings, ReadOnlyDictionary<string, object> members, bool strictMock = false)
            : this(settings, members, new ReadOnlyDictionary<IEnumerable<object>, object>(new Dictionary<IEnumerable<object>, object>()), strictMock)
        {
        }

        public ObjectBase(DxSettings settings, ReadOnlyDictionary<IEnumerable<object>, object> indexes, bool strictMock = false)
            : this(settings, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()), indexes, strictMock)
        {
        }

        public ObjectBase(DxSettings settings, ReadOnlyDictionary<string, object> members, ReadOnlyDictionary<IEnumerable<object>, object> indexes, bool strictMock = false)
        {
            Settings = settings;
            StrictMock = strictMock;
            MockedIndexes = indexes;
            Members = members;
        }

        static TValue ConvertAndReturn<TValue>(object input)
        {
            if (input is MockBuilder)
                return (TValue)(input as MockBuilder).Mock(typeof(TValue));
            else if (input is IPropertyMockAccessor)
                return (input as IPropertyMockAccessor).Get<TValue>();
            else if (!(input is TValue))
                throw new InvalidOperationException("Bad type");
            else
                return (TValue)input;
        }

        /// <summary>
        /// Determine if an object is the desired type or can be converted (not cast) to the desired type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="toTest"></param>
        /// <returns></returns>
        private static bool Is<T>(object toTest)
        {
            return toTest == null && !typeof(T).IsValueType ||
                toTest is T ||
                (toTest is IPropertyMockBuilder<T>) ||
                ((toTest is MockBuilder) && !typeof(T).IsSealed);
        }

        #region Properties

        public bool HasMockedFieldOrProperty<TProperty>(string name)
        {
            return Members.ContainsKey(name) && Is<TProperty>(Members[name]);
        }

        public void SetProperty(string propertyName, object propertyValue)
        {
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
        }

        public TProperty GetProperty<TProperty>(string propertyName)
        {
            TProperty result;
            if (!TryGetProperty(propertyName, out result))
                result = default(TProperty);

            return result;
        }

        public bool TryGetProperty<TProperty>(string propertyName, out TProperty result)
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
                    throw new InvalidOperationException("Property has not been mocked");    //TODE
                }
                else
                {
                    result = default(TProperty);
                    return false;
                }
            }

            result = ConvertAndReturn<TProperty>(property.Value);
            return true;
        }

        #endregion

        #region Indexed Properties

        public IEnumerable<IEnumerable<MethodArg>> GetMockedIndexKeys<TProperty>(IEnumerable<Type> keys)
        {
            var ks = keys.ToArray();
            return MockedIndexes.Where(m => m.Key.Count() == keys.Count() && Is<TProperty>(m.Value))
                .Select(m => new 
                {
                    result = m.Key.Select((k, i) => 
                        (k == null && !ks[i].IsValueType) ||
                        (k != null && ks[i].IsAssignableFrom(k.GetType()))
                    ).All(x => x),
                    keys = m.Key
                })
                .Where(m => m.result)
                .Select(m => m.keys.Select((k, i) => new MethodArg(k, ks[i])))
                .ToArray();
        }

        public TIndexed GetIndex<TIndexed>(IEnumerable<MethodArg> indexValues)
        {
            TIndexed result;
            if (!TryGetIndex(indexValues, out result))
                result = default(TIndexed);

            return result;
        }

        public void SetIndex(IEnumerable<MethodArg> indexValues, object value)
        {
            var values = indexValues.Select(v => v.Arg).ToArray();
            lock (ExtraAddedIndexes)
            {
                if (MockedIndexes.ContainsKey(values) && MockedIndexes[values] is IPropertyMockAccessor)
                {
                    (MockedIndexes[values] as IPropertyMockAccessor).Set(value);
                    return;
                }

                if (ExtraAddedIndexes.ContainsKey(values))
                {
                    ExtraAddedIndexes[values] = value;
                }
                else
                {
                    ExtraAddedIndexes.Add(values, value);
                }
            }
        }

        public bool TryGetIndex<TIndexed>(IEnumerable<MethodArg> indexValues, out TIndexed result)
        {
            KeyValuePair<IEnumerable<object>, object> value;
            lock (ExtraAddedProperties)
            {
                var values = indexValues.Select(v => v.Arg).ToArray();
                value = Indexes.FirstOrDefault(idx =>
                    idx.Key.Count() == values.Length &&
                    idx.Key.Select((k, i) =>  (k == null && values[i] == null) || (k != null && k.Equals(values[i]))).All(a => a));
            }

            if (value.Equals(default(KeyValuePair<IEnumerable<object>, object>)))
            {
                if (StrictMock)
                {
                    throw new InvalidOperationException("Property has not been mocked");    //TODE
                }
                else
                {
                    result = default(TIndexed);
                    return false;
                }
            }

            result = ConvertAndReturn<TIndexed>(value.Value);
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
                .FirstOrDefault(m => m.Value.TryInvoke(genericArguments, arguments.Select(a => a.Arg), out tmp));

            if (method.Equals(default(KeyValuePair<string, MethodGroup>)))
            {
                if (StrictMock)
                {
                    throw new InvalidOperationException("Method has not been mocked");    //TODE
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

            throw new InvalidOperationException("Bad type");    //TODE
        }

        #endregion

        #region Meta

        public class Meta
        {
            public readonly MethodInfo Invoke;
            public readonly MethodInfo InvokeReturnValue;
            public readonly MethodInfo InvokeGeneric;
            public readonly MethodInfo InvokeGenericReturnValue;

            public readonly MethodInfo TryInvoke;
            public readonly MethodInfo TryInvokeReturnValue;
            public readonly MethodInfo TryInvokeGeneric;
            public readonly MethodInfo TryInvokeGenericReturnValue;

            public readonly MethodInfo TryGetProperty;
            public readonly MethodInfo SetProperty;
            public readonly MethodInfo GetProperty;

            public readonly MethodInfo GetIndex;
            public readonly MethodInfo SetIndex;
            public readonly MethodInfo TryGetIndex;
            
            public Meta() 
            {
                Type str = typeof(string), args = typeof(IEnumerable<MethodArg>),
                    gens = typeof(IEnumerable<Type>), vd = typeof(void), bl = typeof(bool);

                var methods = typeof(ObjectBase)
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(m =>new
                    {
                        raw = m,
                        name = m.Name,
                        paramaters = m.GetParameters(),
                        returns = m.ReturnType,
                        genericArgs = m.IsGenericMethodDefinition ? m.GetGenericArguments() : new Type [0]
                    }).ToArray();

                Invoke = methods.Single(m => m.name == "Invoke" && m.paramaters.Length == 2 &&
                    m.paramaters[0].ParameterType == str && m.paramaters[1].ParameterType == args &&
                    m.genericArgs.Length == 0 && m.returns == vd).raw;
                InvokeReturnValue = methods.Single(m => m.name == "Invoke" && m.paramaters.Length == 2 &&
                    m.paramaters[0].ParameterType == str && m.paramaters[1].ParameterType == args &&
                    m.genericArgs.Length == 1 && m.returns == m.genericArgs[0]).raw;
                InvokeGeneric = methods.Single(m => m.name == "Invoke" && m.paramaters.Length == 3 &&
                    m.paramaters[0].ParameterType == str && m.paramaters[1].ParameterType == gens &&
                    m.paramaters[2].ParameterType == args &&
                    m.genericArgs.Length == 0 && m.returns == vd).raw;
                InvokeGenericReturnValue = methods.Single(m => m.name == "Invoke" && m.paramaters.Length == 3 &&
                    m.paramaters[0].ParameterType == str && m.paramaters[1].ParameterType == gens &&
                    m.paramaters[2].ParameterType == args &&
                    m.genericArgs.Length == 1 && m.returns == m.genericArgs[0]).raw;

                TryInvoke = methods.Single(m => m.name == "TryInvoke" && m.paramaters.Length == 2 &&
                    m.paramaters[0].ParameterType == str && m.paramaters[1].ParameterType == args &&
                    m.genericArgs.Length == 0 && m.returns == bl).raw;
                TryInvokeReturnValue = methods.Single(m => m.name == "TryInvoke" && m.paramaters.Length == 3 &&
                    m.paramaters[0].ParameterType == str && m.paramaters[1].ParameterType == args &&
                   // m.paramaters[2].ParameterType == m.genericArgs[0] &&  //TODO
                    m.genericArgs.Length == 1 && m.returns == bl).raw;
                TryInvokeGeneric = methods.Single(m => m.name == "TryInvoke" && m.paramaters.Length == 3 &&
                    m.paramaters[0].ParameterType == str && m.paramaters[1].ParameterType == gens &&
                    m.paramaters[2].ParameterType == args &&
                    m.genericArgs.Length == 0 && m.returns == bl).raw;
                TryInvokeGenericReturnValue = methods.Single(m => m.name == "TryInvoke" && m.paramaters.Length == 4 &&
                    m.paramaters[0].ParameterType == str && m.paramaters[1].ParameterType == gens &&
                    //m.paramaters[2].ParameterType == args && m.paramaters[3].ParameterType == m.genericArgs[0] && //TODO
                    m.genericArgs.Length == 1 && m.returns == bl).raw;

                TryGetProperty = methods.Single(m => m.name == "TryGetProperty").raw;
                SetProperty = methods.Single(m => m.name == "SetProperty").raw;
                GetProperty = methods.Single(m => m.name == "GetProperty").raw;

                GetIndex = methods.Single(m => m.name == "GetIndex").raw;
                SetIndex = methods.Single(m => m.name == "SetIndex").raw;
                TryGetIndex = methods.Single(m => m.name == "TryGetIndex").raw;
            }
        }

        #endregion
    }
}
