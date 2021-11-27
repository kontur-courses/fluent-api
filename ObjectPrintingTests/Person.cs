using System;
using System.Collections.Generic;

namespace ObjectPrintingTests;

public class Person
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public double Height { get; set; }
    public int Age { get; set; }

    public Guid[] OtherPersons { get; set; }

    public Dictionary<DateTime,string> Dates { get; set; }

    public string Biography { get; set; }

    public Person Parent { get; set; }

    public Person Child { get; set; }
}
