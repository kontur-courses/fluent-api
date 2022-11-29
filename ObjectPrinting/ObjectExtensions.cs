using System;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<TOwner>(this TOwner obj)
        {
            return new PrintingConfig<TOwner>().PrintToString(obj);
        }
        
        public static string PrintToString<TOwner>(this TOwner obj, Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> func)
        {
            return func(ObjectPrinter.For<TOwner>()).PrintToString(obj);
        }
    }
}