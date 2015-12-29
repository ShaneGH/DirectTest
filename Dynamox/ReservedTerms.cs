using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks.Info;

namespace Dynamox
{
    /// <summary>
    /// Specifies the method and property names which will be used by Dynamox to perform core actions, such as Return values or Ensure mocks are called
    /// </summary>
    public class ReservedTerms : IReservedTerms
    {
        public string Returns { get; set; }
        public string Ensure { get; set; }
        public string Clear { get; set; }
        public string Do { get; set; }
        public string As { get; set; }
        public string Constructor { get; set; }
        public string Out { get; set; }

        public ReservedTerms()
        {
            Returns = "Returns";
            Ensure = "Ensure";
            Clear = "Clear";
            Do = "Do";
            As = "As";
            Constructor = "Constructor";
            Out = "Out";
        }

        public ReservedTerms(object settings)
            : this()
        {
            if (settings == null)
                return;

            var properties = settings.GetType().GetProperties();
            var fields = settings.GetType().GetFields();

            PropertyInfo p;
            FieldInfo f;

            Action<string, Action<string>> set = (a, b) =>
            {
                if ((p = properties.FirstOrDefault(x => x.Name == a)) != null)
                    b((p.GetValue(settings) ?? a).ToString());
                else if ((f = fields.FirstOrDefault(x => x.Name == a)) != null)
                    b((f.GetValue(settings) ?? a).ToString());
            };

            set("Returns", a => Returns = a);
            set("Ensure", a => Ensure = a);
            set("Clear", a => Clear = a);
            set("Do", a => Do = a);
            set("As", a => As = a);
            set("Constructor", a => Constructor = a);
            set("Out", a => Out = a);
        }
    }
}
