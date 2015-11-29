using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DirectTests
{

    public class MockSettings
    {
        public string Returns { get; set; }
        public string Ensure { get; set; }
        public string Clear { get; set; }
        public string Do { get; set; }
        public string As { get; set; }

        public MockSettings()
        {
            Returns = "Returns";
            Ensure = "Ensure";
            Clear = "Clear";
            Do = "Do";
            As = "As";
        }

        public MockSettings(object settings)
        {
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
        }
    }
}
