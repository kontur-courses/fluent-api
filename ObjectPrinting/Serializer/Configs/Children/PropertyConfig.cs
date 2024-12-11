using System;
using System.Linq.Expressions;
using ObjectPrinting.Tools;

namespace ObjectPrinting.Serializer.Configs.Children;

public class PropertyConfig<TOwner, TPropType>(
    PrintingConfig<TOwner> printingConfig, 
    Expression<Func<TOwner, TPropType>> propertySelector) : IChildConfig<TOwner, TPropType>
{
    public PrintingConfig<TOwner> ParentConfig { get; } = printingConfig;
    public string PropertyName { get; } = propertySelector.TryGetPropertyName();

    public PrintingConfig<TOwner> Using(Func<TPropType, string> print)
    {
        ParentConfig.PropertySerializers[PropertyName] = print;
        return ParentConfig;
    }
}