using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests;

public class PersonV2
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public double Height { get; set; }
    public int Age { get; set; }
    public PersonV2 Mother { get; set; }
    public List<PersonV2> Kids { get; set; }
}