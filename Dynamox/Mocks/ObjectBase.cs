using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    public class ObjectBase
    {
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

        static TValue ConvertAndReturn<TValue>(object input)
        {
            if (input is MockBuilder)
                return (TValue)(input as MockBuilder).Mock(typeof(TValue));
            else if (!(input is TValue))
                throw new InvalidOperationException("Bad type");
            else
                return (TValue)input;
        }

        /// <summary>
        /// Invoke a method with no return value
        /// </summary>
        public void Invoke(string methodName, IEnumerable<KeyValuePair<Type, object>> arguments)
        {
            Invoke(methodName, Enumerable.Empty<Type>(), arguments);
        }

        /// <summary>
        /// Invoke a method with no return value
        /// </summary>
        public void Invoke(string methodName, IEnumerable<Type> genericArguments, IEnumerable<KeyValuePair<Type, object>> arguments)
        {
            TryInvoke(methodName, genericArguments, arguments);
        }

        /// <summary>
        /// Invoke a method with a return value
        /// </summary>
        public TResult Invoke<TResult>(string methodName, IEnumerable<KeyValuePair<Type, object>> arguments)
        {
            return Invoke<TResult>(methodName, Enumerable.Empty<Type>(), arguments);
        }

        /// <summary>
        /// Invoke a method with a return value
        /// </summary>
        public TResult Invoke<TResult>(string methodName, IEnumerable<Type> genericArguments, IEnumerable<KeyValuePair<Type, object>> arguments)
        {
            TResult result;
            TryInvoke(methodName, genericArguments, arguments, out result);
            return result;
        }

        /// <summary>
        /// Invoke a method with no return value
        /// </summary>
        public bool TryInvoke(string methodName, IEnumerable<KeyValuePair<Type, object>> arguments)
        {
            return TryInvoke(methodName, Enumerable.Empty<Type>(), arguments);
        }

        /// <summary>
        /// Invoke a method with no return value
        /// </summary>
        public bool TryInvoke(string methodName, IEnumerable<Type> genericArguments, IEnumerable<KeyValuePair<Type, object>> arguments)
        {
            object dummy = null;
            var method = Methods
                .Where(m => m.Key == methodName)
                .FirstOrDefault(m => m.Value.TryInvoke(genericArguments, arguments.Select(a => a.Value), out dummy));

            if (StrictMock && method.Equals(default(KeyValuePair<string, MethodGroup>)))
                throw new InvalidOperationException("Method has not been mocked");    //TODO

            return method.Equals(default(KeyValuePair<string, MethodGroup>));
        }

        /// <summary>
        /// Invoke a method with a return value
        /// </summary>
        public bool TryInvoke<TResult>(string methodName, IEnumerable<KeyValuePair<Type, object>> arguments, out TResult result)
        {
            return TryInvoke<TResult>(methodName, Enumerable.Empty<Type>(), arguments, out result);
        }

        /// <summary>
        /// Invoke a method with a return value
        /// </summary>
        public bool TryInvoke<TResult>(string methodName, IEnumerable<Type> genericArguments, IEnumerable<KeyValuePair<Type, object>> arguments, out TResult result)
        {
            object tmp = null;
            var method = Methods
                .Where(m => m.Key == methodName)
                .FirstOrDefault(m => m.Value.TryInvoke(genericArguments, arguments.Select(a => a.Value), out tmp));

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
    }
}
