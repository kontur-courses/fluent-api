namespace ObjectPrinting
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj,
                                              int serialiseDepth = ObjectPrinter.DefaultSerialiseDepth,
                                              int sequencesMaxLength = ObjectPrinter.DefaultSequencesMaxLength) =>
            ObjectPrinter.For<T>(serialiseDepth, sequencesMaxLength).PrintToString(obj);
    }
}