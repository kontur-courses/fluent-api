using System.Reflection;

namespace ObjectPrinting
{
    public interface IMemberPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> PrintingConfig { get; }
        MemberInfo MemberInfo { get; }
    }
}