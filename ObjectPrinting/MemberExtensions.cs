namespace ObjectPrinting.Solved;

public static class MemberExtensions
{
    public static IBasicConfigurator<TOwner> TrimByLength<TOwner>(
        this IMemberConfigurator<TOwner, string> propertyConfig, int length)
    {
        return propertyConfig.BasicConfigurator;
    }
}