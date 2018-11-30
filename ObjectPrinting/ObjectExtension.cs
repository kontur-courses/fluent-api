namespace ObjectPrinting
{
    public static class ObjectExtension
    {
        public static string PrintToString(this object obj) =>
            ObjectPrinter.For<object>().PrintToString(obj);
    }
}