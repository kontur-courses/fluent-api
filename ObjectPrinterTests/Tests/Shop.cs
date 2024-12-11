namespace ObjectPrinting.Solved.Tests;

public class Shop(string[] items, string nameShop)
{
    public string NameShop { get; set; } = nameShop;
    public string[] Items { get; set; } = items;


    public static Shop GetShop()
    {
        var items = new string[]
        {
            "Apple",
            "Chips",
            "Orange",
            "Egg",
            "Tomat"
        };

        return new Shop(items, "Пятёрочка");
    }
}