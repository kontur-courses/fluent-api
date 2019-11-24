using System;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectPrinting
{
    public class Config
    {
        public readonly IReadOnlyCollection<Type> FinalTypes;

        public readonly HashSet<MemberInfo> Excluding;
        public readonly Dictionary<MemberInfo, Func<object, string>> Printing;
        public readonly Dictionary<MemberInfo, Func<object, string>> PrintingWithCulture;

        public Config()
        {
            Excluding = new HashSet<MemberInfo>();
            Printing = new Dictionary<MemberInfo, Func<object, string>>();
            PrintingWithCulture = new Dictionary<MemberInfo, Func<object, string>>();

            FinalTypes = new[]
            {
                typeof(int), typeof(double), typeof(float),
                typeof(string),
                typeof(DateTime), typeof(TimeSpan)
            };
        }
    }
}