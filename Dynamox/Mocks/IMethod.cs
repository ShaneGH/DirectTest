using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks
{
    /// <summary>
    /// Describes a method
    /// </summary>
    public interface IMethod
    {
        /// <summary>
        /// Set Ensured to true
        /// </summary>
        IMethod DxEnsure();

        /// <summary>
        /// Add a generic argument to the method definition
        /// </summary>
        IMethod_IGenericAdd AddGeneric<T>();

        /// <summary>
        /// Add a generic argument to the method definition
        /// </summary>
        IMethod_IGenericAdd AddGeneric(Type genericType);

        /// <summary>
        /// Get the generic type args which have been added to this class
        /// </summary>
        IEnumerable<Type> GenericArgs { get; }
    }

    public interface IGenericAdd
    {
        /// <summary>
        /// Add another generic argument to the method definition
        /// </summary>
        IMethod_IGenericAdd And<T>();

        /// <summary>
        /// Add another generic argument to the method definition
        /// </summary>
        IMethod_IGenericAdd And(Type genericType);
    }

    public interface IMethod_IGenericAdd : IMethod, IGenericAdd { }
}
