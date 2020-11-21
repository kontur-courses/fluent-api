namespace ObjectPrinting.Solved
{
    public static class ObjectExtensions
    {
        public static string PrintToString<T>(this T obj)
        {
            return ObjectPrinter.For<T>().PrintToString(obj);
        }

        public static string GetFullNameProperty<TOwner, TPropType>(this Expression<Func<TOwner, TPropType>> memberSelector) =>
            string.Join("", memberSelector.Body.ToString().SkipWhile(c => c != '.'));
    }
}