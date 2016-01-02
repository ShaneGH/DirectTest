using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Compile
{
    // TODO: Implement and use
    public interface ICallProtected
    {
        /// <summary>
        /// Call a protected method
        /// </summary>
        void CallProtectedMethod(string methodName, params object[] ags);

        /// <summary>
        /// Call a protected method with a return value
        /// </summary>
        /// <typeparam name="T">The return value type</typeparam>
        T CallProtectedMethod<T>(string methodName, params object[] ags);
    }
}
