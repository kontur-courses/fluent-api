using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    private Person _person = null!;

    [SetUp]
    public void SetUp()
    {
        var father = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Aaron",
            Secondname = "Smith",
            Age = 56,
            HeightInMeters = 176.3,
            WeightInKilograms = 85.5,
            Children = []
        };

        _person = new Person
        {
            Id = Guid.NewGuid(),
            FirstName = "Alexander",
            Secondname = "Smith",
            Surname = "Junior",
            Age = 20,
            HeightInMeters = 192.54,
            WeightInKilograms = 75.3,
            Father = father
        };

        father.Children.Add(_person);
    }

    [Test]
    public void Demo_ConfiguredPrinter()
    {
        var printer = ObjectPrinter.Setup<Person>()
            .Exclude<Guid>()
            .Exclude(p => p.Children)
            .For<double>()
                .SetCulture(CultureInfo.InvariantCulture)
                .Then()
            .For(p => p.HeightInMeters)
                .SetFormat(height => $"{height.ToString(CultureInfo.InvariantCulture)} см.")
                .Then()
            .For(p => p.FirstName)
                .Truncate(4)
                .Then()
            .Apply();

        var result = printer.PrintToString(_person);

        result.Should().NotContain("Id =");
        result.Should().Contain("WeightInKilograms = 75.3");
        result.Should().Contain("HeightInMeters = 192.54 см.");
        result.Should().Contain("FirstName = Alex");
    }

    [Test]
    public void Demo_DefaultSerialization_And_CyclicReferences()
    {
        var defaultResult = _person.PrintToString();

        defaultResult.Should().Contain(nameof(Person));
        defaultResult.Should().Contain("FirstName = Alexander");
        defaultResult.Should().Contain("[cyclic reference]");
    }

    [Test]
    public void Demo_InlineConfiguration()
    {
        var inlineResult = _person.PrintToString(config => config
            .Exclude<Guid>()
            .For(p => p.Surname)
                .SetFormat(s => s is null ? "Отсутствует" : s)
                .Then()
            .For(p => p.Father)
                .SetFormat(f => f is null ? "Не указан" : $"{f.FirstName} {f.Secondname} (Возраст: {f.Age})")
                .Then());

        inlineResult.Should().NotContain(nameof(Person.Id));
        inlineResult.Should().Contain("Surname = Junior");
        inlineResult.Should().Contain("Father = Aaron Smith (Возраст: 56)");
    }
}
