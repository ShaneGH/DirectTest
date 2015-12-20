using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox
{
    public class DxSettings
    {
        internal static readonly DxSettings GlobalSettings = new DxSettings 
        {
            SetNonVirtualPropertiesOrFields = true
        };

        //TODO: enforce
        public bool SetNonVirtualPropertiesOrFields { get; set; }

        public DxSettings() 
        {
            if (GlobalSettings == null)
                return;

            SetNonVirtualPropertiesOrFields = GlobalSettings.SetNonVirtualPropertiesOrFields;
        }
    }
}
