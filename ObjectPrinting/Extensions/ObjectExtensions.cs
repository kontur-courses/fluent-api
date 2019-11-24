using ObjectPrinting.Configs;

namespace ObjectPrinting.Extensions
{
    public static class ObjectExtensions
    {
        public static string Serialized<TOwner>(this TOwner obj)
        {
            return ObjectPrinter.For<TOwner>().PrintToString(obj);
        }
        
        public static PrintingConfig<TOwner> Serialize<TOwner>(this TOwner obj)
        {
            return ObjectPrinter.For<TOwner>();
        }
    }
}