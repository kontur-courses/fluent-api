using System;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<TOwner>(this TOwner obj, int maxNestingLevel = -1)
        {
            return new PrintingConfig<TOwner>().PrintToString(obj, maxNestingLevel);
        }
        
        public static string PrintToString<TOwner>(this TOwner obj, Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> func, int maxNestingLevel = -1)
        {
            return func(ObjectPrinter.For<TOwner>()).PrintToString(obj, maxNestingLevel);
        }
    }
}