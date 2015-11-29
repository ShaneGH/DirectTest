using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests.Mocks
{
    public class ObjectBase
    {
        readonly bool StrictMock;
        readonly ReadOnlyDictionary<string, object> Members;
        readonly Dictionary<string, object> ExtraAddedProperties = new Dictionary<string,object>();

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

        public ObjectBase(ReadOnlyDictionary<string, object> members, bool strictMock = false)
        {
            StrictMock = strictMock;
            Members = members;
        }

        public ObjectBase(bool strictMock = false)
            : this (new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()), strictMock)
        {
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
            TryGetProperty(propertyName, out result);
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

            if (!(property.Value is TProperty))
                throw new InvalidOperationException("Bad type");

            result = (TProperty)property.Value;
            return true;
        }

        /// <summary>
        /// Invoke a method with no return value
        /// </summary>
        public void Invoke(string methodName, IEnumerable<KeyValuePair<Type, object>> arguments)
        {
            TryInvoke(methodName, arguments);
        }

        /// <summary>
        /// Invoke a method with a return value
        /// </summary>
        public TResult Invoke<TResult>(string methodName, IEnumerable<KeyValuePair<Type, object>> arguments)
        {
            TResult result;
            TryInvoke(methodName, arguments, out result);
            return result;
        }

        /// <summary>
        /// Invoke a method with no return value
        /// </summary>
        public bool TryInvoke(string methodName, IEnumerable<KeyValuePair<Type, object>> arguments)
        {
            object dummy = null;
            var method = Methods
                .Where(m => m.Key == methodName)
                .FirstOrDefault(m => m.Value.TryInvoke(arguments.Select(a => a.Value), out dummy));

            if (StrictMock && method.Equals(default(KeyValuePair<string, MethodGroup>)))
                throw new InvalidOperationException("Method has not been mocked");    //TODO

            return method.Equals(default(KeyValuePair<string, MethodGroup>));
        }

        /// <summary>
        /// Invoke a method with a return value
        /// </summary>
        public bool TryInvoke<TResult>(string methodName, IEnumerable<KeyValuePair<Type, object>> arguments, out TResult result)
        {
            object tmp = null;
            var method = Methods
                .Where(m => m.Key == methodName)
                .FirstOrDefault(m => m.Value.TryInvoke(arguments.Select(a => a.Value), out tmp));

            if (method.Equals(default(KeyValuePair<string, MethodGroup>)))
            {
                if (StrictMock)
                {
                    throw new InvalidOperationException("Method has not been mocked");    //TODO
                }
                else
                {
                    result = default(TResult);
                    return true;
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
