using System;
using System.Collections.Generic;

namespace StringSerializer.Tests.TestModels;

public class TestObject
{
    public int IntNumber { get; set; }
    public double DoubleNumber { get; set; }
    public string? Line { get; set; }
    public DateTime Date { get; set; }
    public TestObject? CircularReference { get; set; }

    public Dictionary<int, TestObject>? Dict { get; set; }

    public List<string> List { get; set; } = new() { "Echo", "Lima", "Omega", "Bravo", "Delta" };
}