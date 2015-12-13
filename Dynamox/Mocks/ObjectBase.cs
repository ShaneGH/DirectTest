using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    public class XXX
    {

        public virtual int DoSomething(int var1, string var2, out bool var3, ref string var4, out decimal var5)
        {
            var5 = 99;
            var3 = true;
            return 3;
        }
        public virtual string DoSomething3(int var1, string var2, out bool var3, ref string var4, out decimal var5)
        {
            var5 = 99;
            var3 = true;
            return "aaaa";
        }

        public virtual int BeSomething
        {
            get;
            set;
        }
    }

    public class ObjectBase : XXX
    {
        public static readonly Meta Reflection = new Meta();

        readonly bool StrictMock;
        readonly ReadOnlyDictionary<string, object> Members;
        readonly Dictionary<string, object> ExtraAddedProperties = new Dictionary<string,object>();
        readonly ReadOnlyDictionary<IEnumerable<object>, object> MockedIndexes;
        readonly Dictionary<IEnumerable<object>, object> ExtraAddedIndexes = new Dictionary<IEnumerable<object>, object>();

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

        public ObjectBase(bool strictMock = false)
            : this(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()), strictMock)
        {
        }

        public ObjectBase(ReadOnlyDictionary<string, object> members, bool strictMock = false)
            : this(members, new ReadOnlyDictionary<IEnumerable<object>, object>(new Dictionary<IEnumerable<object>, object>()), strictMock)
        {
        }

        public ObjectBase(ReadOnlyDictionary<IEnumerable<object>, object> indexes, bool strictMock = false)
            : this(new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()), indexes, strictMock)
        {
        }

        public ObjectBase(ReadOnlyDictionary<string, object> members, ReadOnlyDictionary<IEnumerable<object>, object> indexes, bool strictMock = false)
        {
            StrictMock = strictMock;
            MockedIndexes = indexes;
            Members = members;
        }

        public override int BeSomething
        {
            get
            {
                return base.BeSomething;
            }
            set
            {
                var args = new[]
                {
                    new MethodArg(typeof(int), value)
                };

                SetProperty("BeSomething", args[0].Arg);
            }
        }

        public override int DoSomething(int var1, string var2, out bool var3, ref string var4, out decimal var5)
        {
            int result;
            var args = new[]
            {
                new MethodArg(typeof(int), var1),
                new MethodArg(typeof(string), var2),
                new MethodArg(typeof(bool), null),
                new MethodArg(typeof(string), var4),
                new MethodArg(typeof(int), null)
            };
            if (TryInvoke<int>("DoSomething", args, out result))
            {
                var3 = (bool)args[2].Arg;
                var4 = (string)args[3].Arg;
                var5 = (decimal)args[3].Arg;
            }
            else
            {
                result = base.DoSomething(var1, var2, out var3, ref var4, out var5);
            }

            return result;
        }

        public override string DoSomething3(int var1, string var2, out bool var3, ref string var4, out decimal var5)
        {
            string result;
            var args = new[]
            {
                new MethodArg(typeof(int), var1),
                new MethodArg(typeof(string), var2),
                new MethodArg(typeof(bool), null),
                new MethodArg(typeof(string), var4),
                new MethodArg(typeof(int), null)
            };
            if (TryInvoke<string>("DoSomething", args, out result))
            {
                var3 = (bool)args[2].Arg;
                var4 = (string)args[3].Arg;
                var5 = (decimal)args[3].Arg;
            }
            else
            {
                result = base.DoSomething3(var1, var2, out var3, ref var4, out var5);
            }

            return result;
        }

        static TValue ConvertAndReturn<TValue>(object input)
        {
            if (input is MockBuilder)
                return (TValue)(input as MockBuilder).Mock(typeof(TValue));
            else if (!(input is TValue))
                throw new InvalidOperationException("Bad type");
            else
                return (TValue)input;
        }

        #region Properties

        public void SetProperty(string propertyName, object propertyValue)
        {
            lock (ExtraAddedProperties)
            {
                if (ExtraAddedProperties.ContainsKey(propertyName))
                    ExtraAddedProperties[propertyName] = propertyValue;
                else
                    ExtraAddedProperties.Add(propertyName, propertyValue);
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
                    throw new InvalidOperationException("Property has not been mocked");    //TODO
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

        #region Index

        public TIndexed GetIndex<TIndexed>(IEnumerable<object> indexValues)
        {
            TIndexed result;
            if (!TryGetIndex(indexValues, out result))
                result = default(TIndexed);

            return result;
        }

        public void SetIndex(IEnumerable<object> indexValues, object value)
        {
            var values = indexValues.ToArray();
            lock (ExtraAddedIndexes)
            {
                var kvp = ExtraAddedIndexes.FirstOrDefault(idx =>
                    idx.Key.Count() == values.Length &&
                    idx.Key.Select((k, i) => (k == null && values[i] == null) || (k != null && k.Equals(values[i]))).All(a => a));

                if (!kvp.Equals(default(KeyValuePair<IEnumerable<object>, object>)))
                    ExtraAddedIndexes[kvp.Key] = value;
                else
                    ExtraAddedIndexes.Add(indexValues, value);
            }
        }

        public bool TryGetIndex<TIndexed>(IEnumerable<object> indexValues, out TIndexed result)
        {
            KeyValuePair<IEnumerable<object>, object> value;
            lock (ExtraAddedProperties)
            {
                var values = indexValues.ToArray();
                value = Indexes.FirstOrDefault(idx =>
                    idx.Key.Count() == values.Length &&
                    idx.Key.Select((k, i) =>  (k == null && values[i] == null) || (k != null && k.Equals(values[i]))).All(a => a));
            }

            if (value.Equals(default(KeyValuePair<IEnumerable<object>, object>)))
            {
                if (StrictMock)
                {
                    throw new InvalidOperationException("Property has not been mocked");    //TODO
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
        /// Invoke a method with no return value
        /// </summary>
        public bool TryInvoke(string methodName, IEnumerable<Type> genericArguments, IEnumerable<MethodArg> arguments)
        {
            object dummy = null;
            var method = Methods
                .Where(m => m.Key == methodName)
                .FirstOrDefault(m => m.Value.TryInvoke(genericArguments, arguments.Select(a => a.Arg), out dummy));

            if (StrictMock && method.Equals(default(KeyValuePair<string, MethodGroup>)))
                throw new InvalidOperationException("Method has not been mocked");    //TODO

            return method.Equals(default(KeyValuePair<string, MethodGroup>));
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
                    throw new InvalidOperationException("Method has not been mocked");    //TODO
                }
                else
                {
                    result = default(TResult);
                    return false;
                }
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

            throw new InvalidOperationException("Bad type");    //TODO
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
            }
        }

        #endregion
    }

    public class MethodArg
    {
        public readonly Type ArgType;
        public readonly object Arg;

        public MethodArg(Type argType, object arg)
        {
            ArgType = argType;
            Arg = arg;
        }
    }
}
