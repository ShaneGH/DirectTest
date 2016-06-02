using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks.Info;

namespace Dynamox.Mocks
{
    public class PreBuiltMethod : IMethod_IGenericAdd, IMethodMock
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

        public bool MustBeCalled { get; private set; }

        public bool WasCalled { get; private set; }

        public object ReturnValue { get; private set; }

        public IMethod DxEnsure()
        {
            MustBeCalled = true;
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

        public bool TryInvoke(IEnumerable<Type> genericArguments, IEnumerable<MethodArg> arguments, out object result)
        {
            ReturnValue = result = Method.DynamicInvoke(arguments.Select(a => a.Arg).ToArray());
            WasCalled = true;
            return true;
        }

        public bool RepresentsMethod(MethodInfo method)
        {
            //TODO: Copy pasted
            var methodGenerics = method.GetGenericArguments();
            if (_GenericArgs.Count() != methodGenerics.Length)
            {
                return false;
            }

            for (var i = 0; i < methodGenerics.Length; i++)
            {
                // is constructed generic method
                if (!method.ContainsGenericParameters)
                {
                    if (methodGenerics[i] != _GenericArgs.ElementAt(i))
                        return false;
                }
                else // is generic method
                {
                    var constraints = methodGenerics[i].GetGenericParameterConstraints();
                }
            }

            return MethodApplicabilityChecker.CanMockMethod(method, ArgTypes);
        }

        public IEnumerable<Type> ArgTypes
        {
            get 
            {
                return Method.Method.GetParameters().Select(a => a.ParameterType); 
            }
        }
    }
}