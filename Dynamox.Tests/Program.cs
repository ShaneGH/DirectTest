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
            var ar = new object[] { 3, "", "" };

            var totals = new List<TimeSpan>();
            for (var i = 0; i < 10; i++)
            {
                var t = Time(() =>
                {
                    var cb = new MethodCallback<int, string, string>((a, b, c) => { });
                    cb.Do(ar);
                }, 10000);

                totals.Add(t);
                Console.WriteLine(Math.Round(t.TotalMilliseconds, 3));
            }
            //12
            Console.WriteLine("Average");
            Console.WriteLine(Math.Round(totals.Skip(1).Average(a => a.TotalMilliseconds), 3));
                Console.ReadKey(true);
        }

        public static TimeSpan Time(Action action, int repeat = 1)
        {
            var start = DateTime.Now;

            for (var i = 0; i < repeat; i++)
                action();

            return DateTime.Now - start;
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