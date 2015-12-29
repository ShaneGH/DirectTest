using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Compile;

namespace Dynamox.Mocks
{
    /// <summary>
    /// A group of constructors which will attempt to build an object
    /// </summary>
    public class Constructors : IEnumerable<IConstructor>, IConstructor
    {
        readonly IEnumerable<IConstructor> _Constructors;

        public Constructors(Type forType) 
        {
            if (forType.IsInterface) forType = typeof(object);

            _Constructors = Array.AsReadOnly(forType.GetConstructors()
                // put the empty constructor first, it is most likely to be used
                .OrderBy(c => c.GetParameters().Length)
                .Select(c => !Compiler.IsDxCompiledType(forType) ? //TODO: forType.IsDxCompiledType
                    new NonMockedConstructor(c) :
                    new Constructor(c)).ToArray());
        }

        public object TryConstruct(ObjectBase objectBase, IEnumerable<object> otherArgs)
        {
            return _Constructors.Select(c => c.TryConstruct(objectBase, otherArgs))
                .FirstOrDefault(c => c != null);
        }

        public object Construct(ObjectBase objectBase)
        {
            return Construct(objectBase, Enumerable.Empty<object>());
        }

        public object Construct(ObjectBase objectBase, IEnumerable<object> otherArgs)
        {
            var result = TryConstruct(objectBase, otherArgs);
            if (result == null)
                throw new InvalidOperationException();  //TODE

            return result;
        }

        public IEnumerator<IConstructor> GetEnumerator()
        {
            return _Constructors.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _Constructors.GetEnumerator();
        }
    }
}
