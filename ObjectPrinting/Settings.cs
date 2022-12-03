using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class SerializerSettings
    {
        public List<MemberInfo> MembersToIgnor { get; set; } = new List<MemberInfo>();
        public List<Type> TypesToIgnore { get; set; } = new List<Type>();
        public Dictionary<MemberInfo, Func<object, string>> CustomMembs { get; set; } = new Dictionary<MemberInfo, Func<object, string>>();
        public Dictionary<Type, Func<object, string>> CustomTypes { get; set; } = new Dictionary<Type, Func<object, string>>();
        public Dictionary<Type, IFormatProvider> CustomCultures { get; set; } = new Dictionary<Type, IFormatProvider>();
    }
}
