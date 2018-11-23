using System;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<TOwner>(this TOwner obj)
        {
            return ObjectPrinter.For<TOwner>().PrintToString(obj);
        }

        public static string PrintToString<TOwner>(this TOwner obj, Action<PrintingConfig<TOwner>> serialize)
        {
            return ObjectPrinter.For<TOwner>().PrintToString(obj);
        }
    }
}
