namespace ObjectPrinterTests.Tests;

public class Shop(string[] items, string nameShop)
{
    public string NameShop { get; set; } = nameShop;
    public string[] Items { get; set; } = items;
    public bool IsOpen { get; set; } 

    public static Shop GetShop()
    {
        var items = new[]
        {
            "Apple",
            "Chips",
            "Orange",
            "Egg",
            "Tomat"
        };

        return new Shop(items, "Пятёрочка") {IsOpen = true};
    }
}