﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Dynamox.Builders;
using Dynamox.Compile;
using Dynamox.Mocks;
using Dynamox.Tests.Compile;
using Dynamox.Tests.Features.Mocks;
using Dynamox.Tests.Mocks;

namespace Dynamox.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var tests = new Indexes();

            tests.DynamicPropertyVal_CannotSet();
        }
    }
}