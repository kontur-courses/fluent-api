using System;
using System.Globalization;
using System.Reflection;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropertyType>
    {
        public PrintingConfig<TOwner> Parent { get; }
        public MemberInfo MemberInfo { get; }

        public PrintingConfig<TOwner> As(Func<TPropertyType, string> print)
        {
            Parent.AlternativePrintForProperty[MemberInfo] = print;
            return Parent;
        }

        public PropertyPrintingConfig(PrintingConfig<TOwner> parent, MemberInfo memberInfo)
        {
            Parent = parent;
            MemberInfo = memberInfo;
        }
    }
}
