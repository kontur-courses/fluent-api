namespace ObjectPrintingTests.Domain;

public static class PersonFactory
{
    private static Person AFriend()
        => new() 
        { 
            Id = Guid.Empty, 
            Name = "Bob friend", 
            Age = 20, 
            Height = 200, 
            Birthday = new DateTime(2004, 2, 26), 
            IsStudent = true,
        };
    
    public static Person APersonWithoutId()
        => new() 
        { 
            Id = Guid.Empty, 
            Name = "Bob", 
            Age = 20, 
            Height = 200, 
            Birthday = new DateTime(2004, 2, 26), 
            IsStudent = true,
            Friends = [AFriend(), AFriend()]
        };

    public static Person APersonWithDoubleHeight()
        => new() 
        { 
            Id = Guid.Empty, 
            Name = "Bob", 
            Age = 20, 
            Height = 200.22, 
            Birthday = new DateTime(2004, 2, 26), 
            IsStudent = true 
        };

    public static Person ACyclePerson()
    {
        var person = APersonWithoutId();
        person.Parent = person;
        return person;
    }
}