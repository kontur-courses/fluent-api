using System.Reflection;

namespace ObjectPrinting.PrintingConfigs;

public interface IPropertyPrintingConfig<TOwner, TPropType>
{
    PrintingConfig<TOwner> ParentConfig { get; }
    MemberInfo Member { get; }
}