using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Compile
{
    /// <summary>
    /// Compares name paramter type and number of generics to determine if 2 methods are the same
    /// </summary>
    public class MethodSignatureComparer : IEqualityComparer<MethodInfo>
    {
        public static readonly IEqualityComparer<MethodInfo> Instance = new MethodSignatureComparer();

        private MethodSignatureComparer() { }

        bool IEqualityComparer<MethodInfo>.Equals(MethodInfo x, MethodInfo y)
        {
            if (x == y)
                return true;

            if (x == null || y == null)
                return false;

            if (x.Name != y.Name)
                return false;
            
            ParameterInfo[] xParams = x.GetParameters(), yParams = y.GetParameters();
            if (xParams.Length != yParams.Length)
                return false;

            for (var i = 0; i < xParams.Length; i++)
            {
                if (xParams[i].ParameterType != yParams[i].ParameterType)
                    return false;
            }

            return x.GetGenericArguments().Length == y.GetGenericArguments().Length;
        }

        int IEqualityComparer<MethodInfo>.GetHashCode(MethodInfo obj)
        {
            var stringDescriptor = obj.GetGenericArguments().Length.ToString() + ";" +
                obj.Name + ";" +
                string.Join(",", obj.GetParameters().Select(p => p.ParameterType.GetHashCode().ToString()));

            return stringDescriptor.GetHashCode();
        }
    }
}
