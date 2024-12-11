using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests;

public class Person
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public double Height { get; set; }
    public int Age { get; set; }
    public Person? BestFriend { get; set; }
    public Person[] Friends { get; set; } = [];
    public Dictionary<string, int> BodyParts { get; set; } = new();
}