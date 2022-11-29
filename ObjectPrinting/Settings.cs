using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace ObjectPrinting
{
    public class Settings
    {
        public List<MemberInfo> membersToIgnor;
        public List<Type> typesToIgnore;
        public Dictionary<MemberInfo, Func<object, string>> customMembs;
        public Dictionary<Type, Func<object, string>> customTypes;
        public Dictionary<Type, IFormatProvider> customCultures;

        public Settings()
        {
            membersToIgnor = new List<MemberInfo>();
            typesToIgnore = new List<Type>();
            customMembs = new Dictionary<MemberInfo, Func<object, string>>();
            customTypes = new Dictionary<Type, Func<object, string>>();
            customCultures = new Dictionary<Type, IFormatProvider>();
        }
    }
}
