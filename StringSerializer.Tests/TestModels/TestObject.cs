using System;
using System.Collections.Generic;

namespace StringSerializer.Tests.TestModels;

public class TestObject
{
    public int IntNumber { get; init; }
    public double DoubleNumber { get; init; }
    public string? Line { get; init; }
    public DateTime Date { get; init; }
    public TestObject? CircularReference { get; set; }

    public Dictionary<int, TestObject>? Dict { get; set; }

    public List<string> List { get; } = new() { "Echo", "Lima", "Omega", "Bravo", "Delta" };

    public List<InnerClass> InnerClasses { get; } = new()
    {
        new InnerClass { StringField = "String value", IntField = 42 },
        new InnerClass { StringField = "String value too", IntField = 84 }
    };

    public class InnerClass
    {
        public string StringField { get; set; }
        public int IntField { get; set; }
    }
}