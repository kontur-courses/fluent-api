namespace ObjectPrinting
{
    public static class WrapExtensions
    {
        public static IWrap<TOwner> Trim<TOwner>(
            this IWrap<TOwner> wrap,
            int length)
        {
            return wrap.Wrap(value => StringHelper.Trim(value, length));
        }
    }
}