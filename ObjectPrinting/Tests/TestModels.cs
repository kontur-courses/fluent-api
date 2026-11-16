using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests;

public class Person
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public double Height { get; set; }
    public int Age { get; set; }
    public DateTime BirthDate { get; set; }
    public Address Address { get; set; }
    public List<string> Hobbies { get; set; }
    public Dictionary<string, int> Scores { get; set; }
}

public class Address
{
    public string City { get; set; }
    public string Street { get; set; }
    public int ZipCode { get; set; }
}

public class Node
{
    public string Name { get; set; }
    public Node Parent { get; set; }
    public List<Node> Children { get; set; } = new();
}

public class FieldContainer
{
    public string PublicField;
    public int IntField;
    public Guid GuidField;
}

public class SelfReferencing
{
    public string Name { get; set; }
    public SelfReferencing Myself { get; set; }
}