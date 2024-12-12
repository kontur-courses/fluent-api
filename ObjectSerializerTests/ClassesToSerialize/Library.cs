namespace ObjectSerializerTests.ClassesToSerialize;

public class Library
{
    public Library(string name)
    {
        Name = name;
        Books = new List<Book>();
        BooksByAuthor = new Dictionary<string, string>();
    }

    public void AddBook(Book book)
    {
        Books.Add(book);

        BooksByAuthor[book.Author] = book.Title;
    }

    public readonly string Name;

    public List<Book> Books { get; set; }

    public Dictionary<string, string> BooksByAuthor { get; set; }
}