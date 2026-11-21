using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        public PropertyPrintingConfig<TOwner, TProp> Using(Func<TProp, string> printer)
        {
            Parent.SetMemberPrinter(Member, obj => printer((TProp)obj));
            return this;
        }
    }
}
