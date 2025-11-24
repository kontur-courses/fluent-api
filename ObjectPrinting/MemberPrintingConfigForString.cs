using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public class MemberPrintingConfigForString<TOwner>(PrintingConfig<TOwner> parent, MemberInfo member)
        : MemberPrintingConfig<TOwner, string>(parent, member)
    {
        public PrintingConfig<TOwner> TrimmedToLength(int maxLen)
        {
            Parent.SetMemberTrimLength(Member, maxLen);
            return Parent;
        }
    }
}
