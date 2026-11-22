namespace ObjectPrinting.Tests;

public class Book
{
    public string Title { get; set; }
    public string Author { get; set; }

    public string Description;
    public string PublishingHouse;
    
    public int PublishingYear { get; set; }
    public int Pages;

    public double Rating { get; set; } = 5.0;
}