using System;
using System.Globalization;

namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<TOwner>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
        Func<object, string> PrintingFunction { get; set; }
        CultureInfo Culture { get; set; }
    }
}