using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class CultureSerializationTests
{
    [Test]
    public void ConfigureType_WithCulture_ShouldApplyCultureToAllNumericProperties()
    {
        var person = new Person { Height = 180.5, Weight = 75.3, Salary = 1000.99m };

        var printer = ObjectPrinter.For<Person>()
            .ConfigureType<double>().WithCulture(new CultureInfo("de-DE"))
            .ConfigureType<decimal>().WithCulture(new CultureInfo("fr-FR"));

        var result = printer.PrintToString(person);

        result.Should().Contain("180,5").And.Contain("75,3").And.Contain("1000,99");
    }

    [Test]
    public void ConfigureProperty_WithCulture_ShouldApplyCultureOnlyToSpecificProperty()
    {
        var person = new Person { Height = 180.5, Weight = 75.3 };

        var printer = ObjectPrinter.For<Person>()
            .ConfigureProperty(p => p.Height).WithCulture(new CultureInfo("de-DE"))
            .ConfigureType<double>().WithCulture(new CultureInfo("en-US"));

        var result = printer.PrintToString(person);

        result.Should().Contain("Height = 180,5")  
              .And.Contain("Weight = 75.3");    
    }

    [Test]
    public void Culture_ShouldWorkWithDifferentNumericTypes()
    {
        var obj = new NumericTypes 
        { 
            IntValue = 1000,
            DoubleValue = 1234.56,
            DecimalValue = 789.12m,
            FloatValue = 456.78f
        };

        var printer = ObjectPrinter.For<NumericTypes>()
            .ConfigureType<int>().WithCulture(new CultureInfo("de-DE"))
            .ConfigureType<double>().WithCulture(new CultureInfo("fr-FR"))
            .ConfigureType<decimal>().WithCulture(new CultureInfo("es-ES"))
            .ConfigureType<float>().WithCulture(new CultureInfo("ru-RU"));

        var result = printer.PrintToString(obj);

        result.Should().Contain("1000").And.Contain("1234,56").And.Contain("789,12").And.Contain("456,78");
    }
}

public class NumericTypes
{
    public int IntValue { get; set; }
    public double DoubleValue { get; set; }
    public decimal DecimalValue { get; set; }
    public float FloatValue { get; set; }
}
