namespace ObjectPrinting
{
    public static class ObjectPrinterExtensions
    {
        public static string PrintToString<TOwner>(this TOwner obj)
        {
            if (obj is IConfigForObject confObj)
            {
                return confObj.PrintToString();
            }

            return ObjectPrinter.For<object>().PrintToString(obj);
        }

        public static PrintingConfig<TOwner> Serialize<TOwner>(this TOwner obj)
        {
            return new ConfigForObject<TOwner>(obj);
        }
    }
}
