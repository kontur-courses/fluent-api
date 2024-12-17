using System;
using System.Collections.Generic;

namespace ObjectPrinting.Tests;

public class SimplePerson
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public List<SimplePerson>? Friends { get; set; }
    public Dictionary<int, SimplePerson>? Neighbours { get; set; }
}