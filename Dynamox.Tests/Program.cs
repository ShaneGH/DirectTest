using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dynamox.Builders;
using Dynamox.Compile;
using Dynamox.Mocks;
using Dynamox.Tests.Compile;
using Dynamox.Tests.Features.Mocks;

namespace Dynamox.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new MethodsAndProperties().M_P_M_P();
            new MethodsAndProperties().P_M_P_M();
            new MethodsAndProperties().M_M_M();
            new MethodsAndProperties().P_P_P();
        }
    }
}