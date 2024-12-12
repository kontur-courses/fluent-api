using System.Collections.Generic;

namespace ObjectPrinting.Tests;

public class PersonDatabase
{
    public List<Person> People { get; set; } = new List<Person>();
    public Person Owner { get; set; }
}