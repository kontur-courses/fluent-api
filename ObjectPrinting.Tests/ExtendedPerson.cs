namespace ObjectPrinting.Tests;

public class ExtendedPerson
{
    public Guid Id { get; set; }
    public int Age { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public List<ExtendedPerson> Children { get; set; }
    public char[] Array { get; set; }
    public Dictionary<int, ExtendedPerson> Dict { get; set; }

    public static ExtendedPerson Get()
    {
        var person1 = new ExtendedPerson()
        {
            Id = new Guid(),
            Age = 18,
            FirstName = "Peter",
            LastName = "Gromov",
            Children = new List<ExtendedPerson>(),
            Array = null,
            Dict = null,
        };

        var person2 = new ExtendedPerson()
        {
            Id = new Guid(),
            Age = 20,
            FirstName = "Peter",
            LastName = "Gromov",
            Children = new List<ExtendedPerson>(),
            Array = null,
            Dict = null,
        };
        person1.Children.Add(person2);
        person2.Children.Add(person1);


        return new ExtendedPerson
        {
            Id = new Guid(),
            Age = 22,
            FirstName = "Peter",
            LastName = "Gromov",
            Children = new List<ExtendedPerson>(new[] { person1, person2 }),
            Array = "fds6".ToCharArray(),
            Dict = new Dictionary<int, ExtendedPerson>()
            {
                [3] = person1,
                [2] = null,
            }
        };
    }
}