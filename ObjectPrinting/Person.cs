using System;

namespace ObjectPrinting;

public class Person
{
    // Properties
    public string Name { get; set; }
    public string Surname { get; set; }
    public DateTime DateOfBirth { get; set; }

    // Fields
    public double Height;
    public float Weight;
    public int Iq;
    
    // Can go in recursion
    public Person? Father { get; set; } = null;
    public Person? Mother { get; set; } = null;
    public Person? BestFriend { get; set; } = null;
}