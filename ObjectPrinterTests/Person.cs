﻿namespace ObjectPrinting.Tests;

public class Person
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; }
    public double Height { get; set; }
    public int Age { get; set; }
    public int Defects = 45;
    public string Text = "some text";
}