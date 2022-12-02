using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class SerializerSettings
    {
        public List<MemberInfo> MembersToIgnor { get; set; } = new List<MemberInfo>();
        public List<Type> TypesToIgnore { get; set; } = new List<Type>();
        public Dictionary<MemberInfo, Func<object, string>> CustomMembs { get; set; } = new Dictionary<MemberInfo, Func<object, string>>();
        public Dictionary<Type, Func<object, string>> CustomTypes { get; set; } = new Dictionary<Type, Func<object, string>>();
        public Dictionary<Type, IFormatProvider> CustomCultures { get; set; } = new Dictionary<Type, IFormatProvider>();
        private List<PropertyInfo> PropertiesToIgnor
        {
            get
            {
                var propsToIgnor = MembersToIgnor
                    .Where(x => x is PropertyInfo)
                    .Select(x => (PropertyInfo)x)
                    .ToList();

                return propsToIgnor;
            }
        }
        private List<FieldInfo> FieldsToIgnor
        {
            get
            {
                var fieldsToIgnor = MembersToIgnor
                    .Where(x => x is FieldInfo)
                    .Select(x => (FieldInfo)x)
                    .ToList();

                return fieldsToIgnor;
            }
        }
        private Dictionary<PropertyInfo, Func<object, string>> CustomProperties
        {
            get
            {
                var customProps = CustomMembs
                    .Where(x => x.Key is PropertyInfo).ToDictionary(x => (PropertyInfo)x.Key, y => y.Value);

                return customProps;
            }
        }
        private Dictionary<FieldInfo, Func<object, string>> CustomFields
        {
            get
            {
                var customFields = CustomMembs
                    .Where(x => x.Key is FieldInfo).ToDictionary(x => (FieldInfo)x.Key, y => y.Value);

                return customFields;
            }
        }
    }
}
