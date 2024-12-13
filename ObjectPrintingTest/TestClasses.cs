public class Person
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public int Age { get; set; }
    public double Height { get; set; }
    public DateTime BirthDate { get; set; }
    public Address Address { get; set; }
    public Person Parent { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
}