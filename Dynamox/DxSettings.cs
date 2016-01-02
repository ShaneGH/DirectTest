using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox
{
    public class DxSettings : INotifyPropertyChanged
    {
        internal static readonly DxSettings GlobalSettings = new DxSettings();

        //TODO: enforce
        bool _CreateSealedClasses = true;
        public bool CreateSealedClasses
        {
            get
            {
                return _CreateSealedClasses;
            }
            set
            {
                if (value != _CreateSealedClasses)
                {
                    _CreateSealedClasses = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CreateSealedClasses"));
                }
            }
        }

        //TODO: enforce
        bool _SetNonVirtualPropertiesOrFields = true;
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
        bool _TestForInvalidMocks = false;
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

        bool _CacheTypeCheckers = true;

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

        //TODO: check intellisense comments and enforce
        bool _CheckEventArgs = true;

        /// <summary>
        /// If set to true, will throw an exception if event args do not match previous event subscriptions, if an event is raised
        /// and it's args do not match the subscription args or event subscriptions are mocked as methods or properties. Default: true
        /// </summary>
        public bool CheckEventArgs
        {
            get
            {
                return _CheckEventArgs;
            }
            set
            {
                if (value != _CheckEventArgs)
                {
                    _CheckEventArgs = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("CheckEventArgs"));
                }
            }
        }

        static readonly Action<DxSettings, DxSettings> Copy;
        static DxSettings() 
        {
            ParameterExpression from = Expression.Parameter(typeof(DxSettings)),
                to = Expression.Parameter(typeof(DxSettings));
            var propertiesAndFields = typeof(DxSettings).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetMethod != null && p.SetMethod != null && p.GetMethod.IsPublic && p.SetMethod.IsPublic)
                .Select(p => new { name = p.Name, type = p.PropertyType })
                .Union(typeof(DxSettings).GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => new { name = p.Name, type = p.FieldType }))
                .Where(p => p.type.IsValueType || p.type == typeof(string));

            Copy = Expression.Lambda<Action<DxSettings, DxSettings>>(Expression.Block(propertiesAndFields
                .Select(p => Expression.Assign(Expression.PropertyOrField(to, p.name), Expression.PropertyOrField(from, p.name)))), from, to).Compile();
        }

        public DxSettings()
        {
            if (GlobalSettings != null)
                Copy(GlobalSettings, this);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
