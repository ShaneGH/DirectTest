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
        /// The arg types oof the method
        /// </summary>
        IEnumerable<Type> ArgTypes { get; }

        /// <summary>
        /// The return type of the method
        /// </summary>
        Type ReturnType { get; }

        /// <summary>
        /// If true, this method must be called
        /// </summary>
        bool Ensured { get; }

        /// <summary>
        /// Invoke this method
        /// </summary>
        object Invoke(IEnumerable<object> arguments);

        /// <summary>
        /// Set Ensured to true
        /// </summary>
        IMethod DxEnsure();

        /// <summary>
        /// The generic arguments of the method
        /// </summary>
        IEnumerable<Type> GenericArgs { get; }

        /// <summary>
        /// Add a generic argument to the method definition
        /// </summary>
        IMethod_IGenericAdd AddGeneric<T>();

        /// <summary>
        /// Add a generic argument to the method definition
        /// </summary>
        IMethod_IGenericAdd AddGeneric(Type genericType);
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
