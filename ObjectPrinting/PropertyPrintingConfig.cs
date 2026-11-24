using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner,TProp>
    {
        internal PrintingConfig<TOwner> Parent { get; }
        internal MemberInfo Member { get; }

        internal PropertyPrintingConfig(PrintingConfig<TOwner> parent, MemberInfo member)
        {
            Parent = parent;
            Member = member;
        }

        public PrintingConfig<TOwner> Using(Func<TProp, string> printer)
        {
            Parent.SetMemberPrinter(Member, obj => printer((TProp)obj));
            return Parent;
        }
    }
}
