using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests;

public class Person
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public double Height { get; set; }

    public int Age { get; set; }

    public Person? Parent { get; set; }

    public List<Person>? Friends { get; set; }

    public Dictionary<string, int>? Marks { get; set; }

    public string? Surname;
}