using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks.Info
{
    /// <summary>
    /// Holds the reserved terms for a MockBuilder. The terms are single use only, and are re-set after being passed on to another container
    /// </summary>
    public class ReservedTermsContainer
    {
        IReservedTerms Settings;

        public ReservedTermsContainer(IReservedTerms settings)
        {
            Settings = settings;
        }

        public ReservedTermsContainer()
            : this(null)
        {
        }

        public void Set(IReservedTerms value)
        {
            Settings = value;
        }

        public void Set(ReservedTermsContainer value)
        {
            Set(value != null ? value.Next() : null);
        }

        /// <summary>
        /// Use a value from the settings contained in this object
        /// </summary>
        public T Use<T>(Func<IReservedTerms, T> get)
        {
            return get(Settings ?? new ReservedTerms());
        }

        static readonly IReservedTerms Cache = new ReservedTerms();

        /// <summary>
        /// Use a value from the settings contained in this object. Do not set any values!
        /// </summary>
        internal T Use_Unsafe<T>(Func<IReservedTerms, T> get)
        {
            return get(Settings ?? Cache);
        }

        /// <summary>
        /// Pass the settings onto another object, resetting the settings for this object
        /// </summary>
        /// <returns></returns>
        public IReservedTerms Next()
        {
            try
            {
                return Settings ?? new ReservedTerms();
            }
            finally
            {
                Settings = null;
            }
        }
    }
}
