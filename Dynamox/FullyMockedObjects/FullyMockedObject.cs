using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;
using Dynamox.Mocks.Info;

namespace Dynamox.FullyMockedObjects
{
    public class FullyMockedObject
    {
        static readonly IReservedTerms UnobtrusiveReservedTerms = new ReservedTerms
        {
            DxAs = Guid.NewGuid().ToString(),
            DxClear = Guid.NewGuid().ToString(),
            DxConstructor = Guid.NewGuid().ToString(),
            DxDo = Guid.NewGuid().ToString(),
            DxEnsure = Guid.NewGuid().ToString(),
            DxOut = Guid.NewGuid().ToString(),
            DxReturns = Guid.NewGuid().ToString()
        };

        readonly Type MockType;
        readonly object PropertiesAndMethods;

        public FullyMockedObject(Type mockType, object propertiesAndMethods)
        {
            if (propertiesAndMethods == null)
                throw new ArgumentNullException("propertiesAndMethods");
            if (mockType == null)
                throw new ArgumentNullException("mockType");

            PropertiesAndMethods = propertiesAndMethods;
            MockType = mockType;
        }
        
        readonly object _lock = new object();
        object _Built;
        public object Build() 
        {
            lock(_lock)
            {
                if (_Built == null)
                {
                    BuildAndSet();
                }
            }

            return _Built;
        }

        void BuildAndSet()
        {
            var mock = Dx.Mock(new DxSettings(), UnobtrusiveReservedTerms) as MockBuilder;
            
            var fields = PropertiesAndMethods.GetType().GetFields().Select(f => new 
            {
                name = f.Name,
                value = f.GetValue(PropertiesAndMethods)
            });
            
            var properties = PropertiesAndMethods.GetType().GetProperties().Select(f => new 
            {
                name = f.Name,
                value = f.GetValue(PropertiesAndMethods)
            });
            
            foreach (var p in properties.Concat(fields))
            {
                if (p.value == null || !CheckIfAnonymousType(p.value.GetType()))
                {
                    mock.SetMember(p.name, p.value);
                }
                else
                {
                    var member = (MockType.GetMember(p.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? 
                        Enumerable.Empty<MemberInfo>())
                            .Where(m => m is PropertyInfo || m is FieldInfo)
                            .Select(m =>
                            {
                                if (m is PropertyInfo)
                                    return (m as PropertyInfo).PropertyType;
                                if (m is FieldInfo)
                                    return (m as FieldInfo).FieldType;

                                return null;
                            })
                            .FirstOrDefault();

                    if (member == null)
                    {
                        throw new InvalidMockException("Invalid member \"" + p.name + "\" on type " + MockType.Name);
                    }

                    mock.SetMember(p.name, new FullyMockedObject(member, p.value).Build());
                }
            }

            _Built = mock.Mock(MockType);
        }

        //TODO: copy pasted from http://stackoverflow.com/questions/2483023/how-to-test-if-a-type-is-anonymous
        private static bool CheckIfAnonymousType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            // HACK: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }
    }
}
