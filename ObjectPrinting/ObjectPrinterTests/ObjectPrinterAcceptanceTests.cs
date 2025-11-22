using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using ObjectPrinting.ObjectPrinter;


namespace ObjectPrinting.ObjectPrinterTests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    [Test]
    public void Demo()
    {
        var person = new Person
        {
            Id = Guid.NewGuid(),
            Name = "Alex",
            Age = 19,
            Height = 161.14,
            Scores = [100, 95, 88],
            Tags = ["student", "intern", "dotnet"],
            Ratings = new Dictionary<string, int>
            {
                ["math"] = 5,
                ["cs"] = 5,
                ["english"] = 4
            }
        };
        person.Friend = person;
        
        var printer = ObjectPrinter.ObjectPrinter.For<Person>()
            .Excluding<Guid>()
            .Excluding(p => p.Age)
            .Printing<int[]>().Using(arr => $"int[{arr.Length}] {{ {string.Join(", ", arr)} }}")
            .Printing<List<string>>()
            .Using(list => $"List<string> (Count = {list.Count}) [ {string.Join(", ", list)} ]")
            .Printing<Dictionary<string, int>>().Using(dict =>
            {
                var pairs = dict.Select(kv => $"{kv.Key}: {kv.Value}");
                return $"Dictionary<string,int> (Count = {dict.Count}) {{ {string.Join(", ", pairs)} }}";
            })
            .Printing<int>().Using(n => $"Number: {n}")
            .Printing<double>().Using(CultureInfo.InvariantCulture)
            .Printing(p => p.Name).TrimmedToLength(3)
            .Printing(p => p.Age).TrimmedToLength(1);

        var s1 = printer.PrintToString(person);
        var s2 = person.PrintToString();
        var s3 = person.PrintToString();

        Console.WriteLine(s1);
        Console.WriteLine(s2);
        Console.WriteLine(s3);
    }
}