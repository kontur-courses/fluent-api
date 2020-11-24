using System;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<TOwner>(this TOwner obj)
        {
            return ObjectPrinter.For<TOwner>().PrintToString(obj);
        }

        public static string PrintToString<TOwner>(this TOwner obj,
            Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> config)
        {
            var printer = config(ObjectPrinter.For<TOwner>());
            return printer.PrintToString(obj);
        }
    }
}