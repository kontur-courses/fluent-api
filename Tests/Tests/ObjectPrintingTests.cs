using System.Globalization;
using ObjectPrinting;
using ObjectPrinting.PrintingConfigs.Extensions;
using Tests.TestEntities;

namespace Tests.Tests;

public partial class ObjectPrintingTests
{
    [Test]
    public async Task PrintToString_SimpleObject_ShouldSerializeAllMembers()
    {
        var person = new Person { Name = "John", Age = 25 };
        var result = person.Print();
        
        await Verify(result);
    }

    [Test]
    public async Task PrintToString_WithMaximumConfigurations_ShouldApplyAllCorrectly()
    {
        var testData = new SuperComplexClass
        {
            ShortName = "John",
            VeryLongDescription = "This is a very long description that should be trimmed because it exceeds the maximum allowed length for this property",
            Age = 35,
            Weight = 75.5,
            Salary = 75000.50m,
            Bonus = 15000.25m,
            BirthDate = new DateTime(1988, 5, 15, 10, 30, 0),
            UserId = Guid.Parse("12345678-1234-1234-1234-123456789012"),
            IsActive = true,
            Tags = new List<string> { "developer", "senior", "backend", "very-long-tag-that-should-be-trimmed" },
            Scores = new Dictionary<string, int>
            {
                ["C#"] = 95,
                ["SQL"] = 88,
                ["JavaScript"] = 76
            },
            TeamMembers = new List<SuperComplexClass>
            {
                new() { ShortName = "Alice", Age = 28, Salary = 60000.00m },
                new() { ShortName = "Bob", Age = 32, Salary = 65000.00m }
            },
            Manager = new SuperComplexClass { ShortName = "Manager", Age = 45 }
        };

        testData.Manager.TeamMembers.Add(testData);
        testData.Assistant = testData.TeamMembers[0]; 
    
        var printer = ObjectPrinter.For<SuperComplexClass>()
            .Excluding<bool>()
            .Excluding<Guid>()
            .Excluding(x => x.Weight)
            .Excluding(x => x.Bonus)
            .Printing<int>().Using(i => $"{i} years")
            .Printing<double>().Using(d => $"{d} kg")
            .Printing<decimal>().Using(d => $"{d:C2}")
            .Printing<DateTime>().Using(dt => dt.ToString("yyyy-MM-dd"))
            .Printing<string>().Using(s => s.ToUpper())
            .Printing(x => x.Salary).Using(s => $"Salary: {s}$")
            .Printing(x => x.Age).Using(a => $"Age is {a}")
            .Printing(x => x.ShortName).TrimToLength(2)
            .Printing(x => x.VeryLongDescription).TrimToLength(20)
            .Printing<decimal>().Using(new CultureInfo("de-DE"))
            .Printing<double>().Using(new CultureInfo("fr-FR"))
            .Printing<List<string>>().Using(list => $"Tags[{list.Count}]")
            .Printing<Dictionary<string, int>>().Using(dict => $"Scores[{dict.Count}]")
            .Create();

        var result = printer.PrintToString(testData);

        await Verify(result);
    }
}