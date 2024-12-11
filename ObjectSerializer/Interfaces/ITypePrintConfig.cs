using System.Globalization;

namespace ObjectSerializer.Interfaces;

public interface ITypePrintConfig<TOwner, TType>
{
    IPrintingConfig<TOwner> Using(CultureInfo cultureInfo);

    IPrintingConfig<TOwner> Using(Func<TType, string> printTypeFunc);
}