using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    internal class PrintingSettings
    {
        private readonly HashSet<Type> excludedTypes = new();
        private readonly HashSet<MemberInfo> excludedMembers = new();
        private readonly Dictionary<Type, Func<object, string>> customPrinters = new();
        private readonly Dictionary<Type, CultureInfo> customCultures = new();
        private readonly Dictionary<MemberInfo, Func<object, string>> memberPrinters = new();

        public void ExcludeType(Type type) => excludedTypes.Add(type);
        public bool IsTypeExcluded(Type type) => excludedTypes.Contains(type);

        public void ExcludeMember(MemberInfo member) => excludedMembers.Add(member);
        public bool IsMemberExcluded(MemberInfo member) => excludedMembers.Contains(member);

        public void SetTypePrinter(Type type, Func<object, string> printer) => customPrinters[type] = printer;
        public bool TryGetTypePrinter(Type type, out Func<object, string> printer)
            => customPrinters.TryGetValue(type, out printer);

        public void SetCulture(Type type, CultureInfo culture) => customCultures[type] = culture;
        public bool TryGetCulture(Type type, out CultureInfo culture)
            => customCultures.TryGetValue(type, out culture);

        public void SetMemberPrinter(MemberInfo member, Func<object, string> printer) => memberPrinters[member] = printer;
        public bool TryGetMemberPrinter(MemberInfo member, out Func<object, string> printer)
            => memberPrinters.TryGetValue(member, out printer);
    }
}
