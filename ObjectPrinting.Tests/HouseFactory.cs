namespace ObjectPrinting.Tests
{
    public static class HouseFactory
    {
        public static House Get() =>
            new()
            {
                Address = "New-York"
            };
    }
}