using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class SerializationConfig
    {
        public readonly HashSet<Type> ExcludedTypes;
        public readonly HashSet<MemberInfo> ExcludedMembers;
        public readonly Dictionary<Type, Func<object, string>> TypePrintingRules;
        public readonly Dictionary<MemberInfo, Func<object, string>> MemberPrintingRules;
        public readonly Dictionary<Type, CultureInfo> TypeCultures;
        public readonly Dictionary<MemberInfo, CultureInfo> MemberCultures;
        public readonly Dictionary<MemberInfo, Func<string, string>> MemberTrimToLength;
        public readonly Func<string, string> GlobalTrimToLength;
        
        public readonly Type[] FinalTypes =
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };


        public SerializationConfig(HashSet<Type> excludedTypes, HashSet<MemberInfo> excludedMembers,
            Dictionary<Type, Func<object, string>> typePrintingRules,
            Dictionary<MemberInfo, Func<object, string>> memberPrintingRules,
            Dictionary<Type, CultureInfo> typeCultures, Dictionary<MemberInfo, CultureInfo> memberCultures,
            Dictionary<MemberInfo, Func<string, string>> memberTrimToLength, Func<string, string> globalTrimToLength)
        {
            ExcludedTypes = excludedTypes;
            ExcludedMembers = excludedMembers;
            TypePrintingRules = typePrintingRules;
            MemberPrintingRules = memberPrintingRules;
            TypeCultures = typeCultures;
            MemberCultures = memberCultures;
            MemberTrimToLength = memberTrimToLength;
            GlobalTrimToLength = globalTrimToLength;
        }
    }
}