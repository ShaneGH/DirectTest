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
            SetNonVirtualPropertiesOrFields = true,
            TestForInvalidMocks = true
        };

        //TODO: enforce
        public bool SetNonVirtualPropertiesOrFields { get; set; }

        //TODO: enforce
        public bool TestForInvalidMocks { get; set; }

        public DxSettings() 
        {
            if (GlobalSettings == null)
                return;

            SetNonVirtualPropertiesOrFields = GlobalSettings.SetNonVirtualPropertiesOrFields;
            TestForInvalidMocks = GlobalSettings.TestForInvalidMocks;
        }
    }
}
