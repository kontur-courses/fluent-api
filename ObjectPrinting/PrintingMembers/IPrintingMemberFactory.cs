using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectPrinting.PrintingMembers
{
    public interface IPrintingMemberFactory
    {
        IEnumerable<MemberTypes> SupportedTypes { get; }
        PrintingMember Convert(MemberInfo memberInfo);
        public bool CanConvert(MemberInfo memberInfo) => SupportedTypes.Contains(memberInfo.MemberType);
    }
}