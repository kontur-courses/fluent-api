using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    internal class GlobalConfig
    {
        public Type[] FinalTypes = new Type[0];
        public readonly List<MemberInfo> ExcludedMembers = new List<MemberInfo>();

        public readonly Dictionary<MemberInfo, Func<object, string>> AlternativeMemberSerializations =
            new Dictionary<MemberInfo, Func<object, string>>();

        public readonly Dictionary<MemberInfo, CultureInfo> Cultures =
            new Dictionary<MemberInfo, CultureInfo>();

        public readonly Dictionary<MemberInfo, int> CutLengths = 
            new Dictionary<MemberInfo, int>();

        public CultureInfo DefaultCulture;
        public int DefaultCutLength = int.MaxValue;
    }
}