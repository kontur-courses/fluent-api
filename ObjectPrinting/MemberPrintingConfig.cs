using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPrinting
{
    public class MemberPrintingConfig<TOwner, TProp>(PrintingConfig<TOwner> parent, MemberInfo member)
    {
        protected readonly PrintingConfig<TOwner> Parent = parent;
        protected readonly MemberInfo Member = member;

        public PrintingConfig<TOwner> Using(Func<TProp, string> serializer)
        {
            Parent.SetMemberSerializer(Member, serializer);
            return Parent;
        }

    }
}
