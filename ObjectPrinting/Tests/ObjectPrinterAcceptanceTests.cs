using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    [Test]
    public void Demo()
    {
        var person = new Person
        {
            Name = "Alex",
            Age = 19,
            Height = 185.12,
            Tags = ["tag1", "tag2"],
            CustomProperties = new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" } },
        };

        var printer = ObjectPrinter.For<Person>()
            .Exclude<Guid>()
            .Exclude(p => p.Age)
            .SpecifyCulture(p => p.LastTimeOnline, CultureInfo.GetCultureInfo("es-ES"))
            .AddSerializer(p => p.Height, _ => "2005")
            .Trim(p => p.Name, 2)
            .Build();

        var s1 = printer.PrintToString(person);
        Console.WriteLine(s1);
    }
}