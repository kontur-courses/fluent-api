using System;

namespace ObjectPrinting;

public interface IPropertyPrintingConfig<TOwner, TPropType>
{
    PrintingConfig<TOwner> Using(Func<TPropType, string> serializer);
}