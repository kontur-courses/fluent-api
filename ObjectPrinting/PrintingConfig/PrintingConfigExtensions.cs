using System;

namespace ObjectPrinting
{
    public static class PrintingConfigExtensions
    {
        public static string Print<TOwner>(this TOwner obj)
        {
            return ObjectPrinter.For<TOwner>().PrintToString(obj);
        }

        public static string Print<TOwner>(this TOwner obj, Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> configurator)
        {
            var printer = ObjectPrinter.For<TOwner>();
            return configurator(printer).PrintToString(obj);
        }
    }
}