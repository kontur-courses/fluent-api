using System;
using System.Reflection;

namespace ObjectPrinting
{
    public interface IMemberPrintingConfig<TOwner, TMemType>
    {
        public PrintingConfig<TOwner> ParentConfig { get; }
        public MemberInfo MemberInfo { get; }
    }
}