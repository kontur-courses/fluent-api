using System;

namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string Serialize<TOwner>(this TOwner owner,
            Func<PrintingConfig<TOwner>, PrintingConfig<TOwner>> config = null)
        {
            return config != null
                ? config(ObjectPrinter.For<TOwner>()).PrintToString(owner)
                : ObjectPrinter.For<TOwner>().PrintToString(owner);
        }
    }
}
