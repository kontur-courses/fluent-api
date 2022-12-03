using System;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropertyType>
    {
        public PropertyPrintingConfig(PrintingConfig<TOwner> parent, MemberInfo memberInfo)
        {
            Parent = parent;
            MemberInfo = memberInfo;
        }

        public PrintingConfig<TOwner> Parent { get; }
        public MemberInfo MemberInfo { get; }

        public PrintingConfig<TOwner> As(Func<TPropertyType, string> print)
        {
            if (Parent.AlternativePrintForMembers.ContainsKey(MemberInfo))
                throw new InvalidOperationException("An alternative serialization method for this property/field has already been set before");
            Parent.AlternativePrintForMembers[MemberInfo] = print;
            return Parent;
        }
    }
}