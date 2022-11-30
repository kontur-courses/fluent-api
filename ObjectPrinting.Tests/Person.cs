namespace ObjectPrinting.Tests;

public class Person
{
    // Fields
    public double Height;
    public float Weight;
    public int Iq;


    // Properties
    public string Name { get; set; }
    public string Surname { get; set; }
    public DateTime DateOfBirth { get; set; }

    // Can go in recursion
    public Person? Father { get; set; } = null;
    public Person? Mother { get; set; } = null;
    public Person? BestFriend { get; set; } = null;
}