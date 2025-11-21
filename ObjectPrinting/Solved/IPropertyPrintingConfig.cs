using System.Reflection;

namespace ObjectPrinting.Solved;

public interface IPropertyPrintingConfig<TOwner, TPropType>
{
    public PrintingConfig<TOwner> ParentConfig { get; }
    public MemberInfo? MemberInfo { get; }
}