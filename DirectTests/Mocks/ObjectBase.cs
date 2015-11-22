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
                return ExtraAddedProperties.Union(
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
            KeyValuePair<string, object> property;
            lock (ExtraAddedProperties)
            {
                property = Properties.FirstOrDefault(p => p.Key == propertyName);
            }

            if (property.Equals(default(KeyValuePair<string, object>)))
            {
                if (StrictMock)
                    throw new InvalidOperationException("Property has not been mocked");    //TODO
                else
                    return default(TProperty);
            }

            if (!(property.Value is TProperty))
                throw new InvalidOperationException("Bad type");

            return (TProperty)property.Value;
        }

        /// <summary>
        /// Invoke a method with no return value
        /// </summary>
        public void Invoke(string methodName, IEnumerable<KeyValuePair<Type, object>> arguments)
        {
            object dummy = null;
            var method = Methods
                .Where(m => m.Key == methodName)
                .FirstOrDefault(m => m.Value.TryInvoke(arguments.Select(a => a.Value), out dummy));

            if (StrictMock && method.Equals(default(KeyValuePair<string, MethodGroup>)))
                throw new InvalidOperationException("Method has not been mocked");    //TODO
        }

        /// <summary>
        /// Invoke a method with a return value
        /// </summary>
        public TResult Invoke<TResult>(string methodName, IEnumerable<KeyValuePair<Type, object>> arguments)
        {
            object result = null;
            var method = Methods
                .Where(m => m.Key == methodName)
                .FirstOrDefault(m => m.Value.TryInvoke(arguments.Select(a => a.Value), out result));

            if (method.Equals(default(KeyValuePair<string, MethodGroup>)))
            {
                if (StrictMock)
                    throw new InvalidOperationException("Method has not been mocked");    //TODO
                else
                    return default(TResult);
            }

            if (result is TResult || (result == null && !typeof(TResult).IsValueType))
                return (TResult)result;
            if (result == null)
                //TODO: currently cannot do strict mocks here.
                return default(TResult);

            throw new InvalidOperationException("Bad type");    //TODO
        }
    }
}
