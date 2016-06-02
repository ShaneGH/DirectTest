using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    public abstract class PreBuiltMethod : IMethod_IGenericAdd
    {
        readonly List<Type> _GenericArgs = new List<Type>();
        readonly Delegate Method;

        public IEnumerable<Type> GenericArgs
        {
            get
            {
                return _GenericArgs.Skip(0);
            }
        }

        public PreBuiltMethod(Delegate method)
        {
            Method = method;
        }

        public object Invoke(IEnumerable<object> arguments)
        {
            return Method.DynamicInvoke(arguments.ToArray());
        }

        public abstract IEnumerable<Type> ArgTypes { get; }

        public abstract Type ReturnType { get; }

        public bool Ensured { get; private set; }

        public IMethod DxEnsure()
        {
            Ensured = true;
            return this;
        }

        public IMethod_IGenericAdd AddGeneric(Type genericType)
        {
            if (genericType == null)
                throw new ArgumentNullException("genericType");

            _GenericArgs.Add(genericType);
            return this;
        }
        
        IMethod_IGenericAdd IMethod.AddGeneric<T>()
        {
            return AddGeneric(typeof(T));
        }
        
        IMethod_IGenericAdd IGenericAdd.And<T>()
        {
            return AddGeneric(typeof(T));
        }
        
        IMethod_IGenericAdd IGenericAdd.And(Type genericType)
        {
            return AddGeneric(genericType);
        }
    }
}
