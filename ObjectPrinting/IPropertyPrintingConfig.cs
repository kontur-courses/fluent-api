using System;

namespace ObjectPrinting
{
    public interface IPropertyPrintingConfig<in TProp>
    {
         Func<TProp, string> PrintingFunction { get; }
    }
}