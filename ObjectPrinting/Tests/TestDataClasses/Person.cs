using System;
using System.Collections.Generic;


namespace ObjectPrinting.Tests;

public class Person
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public double Height { get; set; }
    public string Email { get; set; }
    public Person Parent { get; set; }
    public List<Person> Children { get; set; } = [];
}