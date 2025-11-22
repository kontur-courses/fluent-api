using System.Reflection;

namespace Homework;

public interface IPropertyPrintingConfig<TOwner, TPropType>
{
    PrintingConfig<TOwner> ParentConfig { get; }
    MemberInfo? MemberInfo { get; }
}
