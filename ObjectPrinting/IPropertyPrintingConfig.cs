using System;
using System.Globalization;

namespace ObjectPrinting;

public interface IPropertyPrintingConfig<TOwner, TProp> : IPrintingConfig<TOwner>
{
    IPropertyPrintingConfig<TOwner, TProp> UsingSerializer(Func<TProp, string> serializer);

    IPropertyPrintingConfig<TOwner, TProp> UsingCulture(CultureInfo culture);

    IPropertyPrintingConfig<TOwner, TProp> TrimmedToLength(int maxLength);
}