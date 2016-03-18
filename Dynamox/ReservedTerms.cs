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
        public static readonly ReservedTerms Default = new ReservedTerms 
        {
            DxReturns = "DxReturns",
            DxEnsure = "DxEnsure",
            DxClear = "DxClear",
            DxDo = "DxDo",
            DxAs = "DxAs",
            DxConstructor = "DxConstructor",
            DxOut = "DxOut"
        };

        public string DxReturns { get; set; }
        public string DxEnsure { get; set; }
        public string DxClear { get; set; }
        public string DxDo { get; set; }
        public string DxAs { get; set; }
        public string DxConstructor { get; set; }
        public string DxOut { get; set; }

        public ReservedTerms()
        {
            if (Default == null)
                return;

            DxReturns = Default.DxReturns;
            DxEnsure = Default.DxEnsure;
            DxClear = Default.DxClear;
            DxDo = Default.DxDo;
            DxAs = Default.DxAs;
            DxConstructor = Default.DxConstructor;
            DxOut = Default.DxOut;
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

            set("DxReturns", a => DxReturns = a);
            set("DxEnsure", a => DxEnsure = a);
            set("DxClear", a => DxClear = a);
            set("DxDo", a => DxDo = a);
            set("DxAs", a => DxAs = a);
            set("DxConstructor", a => DxConstructor = a);
            set("DxOut", a => DxOut = a);
        }
    }
}
