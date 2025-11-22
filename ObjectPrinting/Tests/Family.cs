using System.Collections.Generic;

namespace ObjectPrinting.Tests;

public class Family
{
    public Person[] GrandParents { get; set; }
    public List<Person> Parents { get; set; }
    public Dictionary<int, Person> Children  { get; set; }
    public Person Father { get; set; }
    public Person Mother { get; set; }
    public double Age { get; set; }
}