using System;

namespace ObjectPrinting;

public interface ITypePrintingConfig<TOwner, TPropType>
{
    PrintingConfig<TOwner> Using(Func<TPropType, string> serializer);
}