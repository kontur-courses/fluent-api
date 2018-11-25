using System;

namespace ObjectPrinting
{
    public static class ObjectExtension
    {
        public static string PrintToString<TOwner>(this TOwner obj)
        {
            return ObjectPrinter.For<TOwner>().PrintToString(obj);
        }

        public static string PrintToString<TOwner>(this TOwner obj,
            Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> configuringFunction)
        {
            return configuringFunction(ObjectPrinter.For<TOwner>()).PrintToString(obj);
        }
    }
}
