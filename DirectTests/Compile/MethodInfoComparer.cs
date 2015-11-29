using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests.Compile
{
    /// <summary>
    /// Compares name paramter type and return type to determine if 2 methods are the same
    /// </summary>
    public class MethodInfoComparer : IEqualityComparer<MethodInfo>
    {
        public static readonly IEqualityComparer<MethodInfo> Instance = new MethodInfoComparer();

        private MethodInfoComparer() { }

        bool IEqualityComparer<MethodInfo>.Equals(MethodInfo x, MethodInfo y)
        {
            if (x == y)
                return true;

            if (x == null || y == null)
                return false;
            
            ParameterInfo[] xParams = x.GetParameters(), yParams = y.GetParameters();
            if (xParams.Length != yParams.Length)
                return false;

            for (var i = 0; i < xParams.Length; i++)
            {
                if (xParams[i].ParameterType != yParams[i].ParameterType)
                    return false;
            }

            return x.ReturnType == y.ReturnType && x.Name == y.Name;
        }

        int IEqualityComparer<MethodInfo>.GetHashCode(MethodInfo obj)
        {
            var stringDescriptor = obj.ReturnType.GetHashCode().ToString() + ";" +
                obj.Name.GetHashCode().ToString() + ";" +
                string.Join(",", obj.GetParameters().Select(p => p.ParameterType.GetHashCode().ToString()));

            return stringDescriptor.GetHashCode();
        }
    }
}
