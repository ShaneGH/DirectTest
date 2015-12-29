using System;
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
using Dynamox.Tests.SmokeTests;

namespace Dynamox.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var tmp = new SettingsTests();
            tmp.Returns();
        }
    }

    public abstract class Abs
    {
        public abstract event EventHandler Event;
    }

    public interface IXXX
    {
        event EventHandler Event;
    }

    public class Con : IXXX
    {
        public event EventHandler Event;
    }

    //public class C2 : C1
    //{
    //    public void RaiseEvent1(string eventName, object[] args)
    //    {
    //        if (eventName == "Event")
    //            if (Event != null)
    //                Event((object)args[0], (EventArgs)args[1]);
    //    }

    //    public override event EventHandler Event;
    //}
}