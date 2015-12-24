using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox
{
    public class DxSettings : INotifyPropertyChanged
    {
        internal static readonly DxSettings GlobalSettings = new DxSettings
        {
            _SetNonVirtualPropertiesOrFields = true,
            _TestForInvalidMocks = true,
            _CacheTypeCheckers = true
        };

        //TODO: enforce
        bool _SetNonVirtualPropertiesOrFields;
        public bool SetNonVirtualPropertiesOrFields
        {
            get
            {
                return _SetNonVirtualPropertiesOrFields;
            }
            set
            {
                if (value != _SetNonVirtualPropertiesOrFields)
                {
                    _SetNonVirtualPropertiesOrFields = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("SetNonVirtualPropertiesOrFields"));
                }
            }
        }

        //TODO: enforce
        bool _TestForInvalidMocks;
        public bool TestForInvalidMocks
        {
            get
            {
                return _TestForInvalidMocks;
            }
            set
            {
                if (value != _TestForInvalidMocks)
                {
                    _TestForInvalidMocks = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("TestForInvalidMocks"));
                }
            }
        }

        bool _CacheTypeCheckers;

        /// <summary>
        /// There are several reflection based objects which create and validate a mocked class. 
        /// If set to true these items will be cached to speed up testing. Should be true for all but the largest of projects
        /// </summary>
        public bool CacheTypeCheckers
        {
            get
            {
                return _CacheTypeCheckers;
            }
            set
            {
                if (value != _CacheTypeCheckers)
                {
                    _CacheTypeCheckers = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CacheTypeCheckers"));
                }
            }
        }

        public DxSettings() 
        {
            if (GlobalSettings == null)
                return;

            _SetNonVirtualPropertiesOrFields = GlobalSettings.SetNonVirtualPropertiesOrFields;
            _TestForInvalidMocks = GlobalSettings.TestForInvalidMocks;
            _CacheTypeCheckers = GlobalSettings.CacheTypeCheckers;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
