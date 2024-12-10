﻿using System;

namespace ObjectPrinting.Tests;

public class Person
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string SecondName { get; set; }
    public double Height { get; set; }
    public double Weight { get; set; }
    public int Age { get; set; }
    public string Nickname;

    public DateTime Birthday { get; set; }
}

public class PersonWithChild : Person
{
    public PersonWithChild Child { get; set; }
}