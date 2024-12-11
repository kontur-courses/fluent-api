namespace ObjectSerializerTests.ClassesToSerialize;

public class Library
{
    public Library()
    {
        Books = new List<Book>();
        BooksByAuthor = new Dictionary<string, List<Book>>();
    }

    public List<Book> Books { get; set; }

    public Dictionary<string, List<Book>> BooksByAuthor { get; set; }
}