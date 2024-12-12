using System.Globalization;
using FluentAssertions;

namespace ObjectPrinting.Tests;

public class PrintingConfigTests
{
    private readonly Person defaultPerson = new()
    {
        Name = "Alex",
        Age = 19,
        Email = "alex@gmail.com",
        Height = 185.5
    };

    [Test]
    public void Excluding_MustExcludeFieldsAndTypePropertiesFromSerialization()
    {
        var expected = File.ReadAllText("ExpectedResponses/Excluding_MustExcludeFieldsAndTypePropertiesFromSerialization.txt");
        var printer = ObjectPrinter.For<Person>()
            .Excluding<string>();
        
        var actual = printer.PrintToString(defaultPerson);
        
        actual.Should().Be(expected);
    }
    
    [Test]
    public void Excluding_FullObjectSerialization()
    {
        var expected = File.ReadAllText("ExpectedResponses/Excluding_FullObjectSerialization.txt");
        var printer = ObjectPrinter.For<Person>();
        
        var actual = printer.PrintToString(defaultPerson);
        
        actual.Should().Be(expected);
    }

    [Test]
    public void Using_AnAlternativeWayToSerializeNumbers()
    {
        var expected = File.ReadAllText("ExpectedResponses/Using_AnAlternativeWayToSerializeNumbers.txt");
        var printer = ObjectPrinter.For<Person>()
            .Printing<int>()
            .Using(number => Convert.ToString(number, 2));
        
        var actual = printer.PrintToString(defaultPerson);
        
        actual.Should().Be(expected);
    }
    
    [Test]
    public void Excluding_NamePropertyIsExcluded()
    {
        var expected = File.ReadAllText("ExpectedResponses/Excluding_NamePropertyIsExcluded.txt");
        var printer = ObjectPrinter.For<Person>()
            .Excluding(p => p.Name);
        
        var actual = printer.PrintToString(defaultPerson);
        
        actual.Should().Be(expected);
    }
    
    [Test]
    public void Using_OtherLocalizationOfEmailField()
    {
        var expected = File.ReadAllText("ExpectedResponses/Using_OtherLocalizationOfEmailField.txt");
        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.Email)
            .Using(email => email.ToUpper() + Environment.NewLine);
        
        var actual = printer.PrintToString(defaultPerson);
        
        actual.Should().Be(expected);
    }
    
    [Test]
    public void Using_NamePropertyIsTruncated()
    {
        var person = new Person
        {
            Name = "Хьюберт Блейн Вольфешлегельштейнхаузенбергердорф-старший",
            Height = defaultPerson.Height,
            Age = defaultPerson.Age,
            Email = defaultPerson.Email
        };
        var expected = File.ReadAllText("ExpectedResponses/Using_NamePropertyIsTruncated.txt");
        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.Name)
            .TrimmedToLength(20);
        
        var actual = printer.PrintToString(person);
        
        actual.Should().Be(expected);
    }
    
    [Test]
    public void Using_ChangedTypeSerializationCulture()
    {
        var expected = File.ReadAllText("ExpectedResponses/Using_ChangedTypeSerializationCulture.txt");
        var printer = ObjectPrinter.For<Person>()
            .Printing<double>()
            .Using(new CultureInfo("en-US"));
        
        var actual = printer.PrintToString(defaultPerson);
        
        actual.Should().Be(expected);
    }
}