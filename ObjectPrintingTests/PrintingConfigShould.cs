using System.Globalization;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework.Internal;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class PrintingConfigShould
{
    private readonly PrintingConfig<Person> personPrintingConfig = ObjectPrinter.For<Person>();
    private readonly Person alex = new(Guid.NewGuid(), "Alex", 188, 111, DateTime.MinValue);

    [Test]
    public void PropertyPrintingConfig_SetSerializationMethodForProperty()
    {
        var fullText = personPrintingConfig
            .Printing(p => p.Height)
            .Using(h => $"{h} meters")
            .PrintToString(alex);
        var expectedHeightString = $"{alex.Height} meters";
        
        fullText
            .Should()
            .Contain(expectedHeightString);
    }
    
    [TestCase("X")]
    [TestCase("C")]
    [TestCase("F")]
    public void PropertyPrintingConfig_ChangeSerializationMethod(string format)
    {
        var fullText = personPrintingConfig
            .Printing<int>()
            .Using(i => i.ToString(format))
            .PrintToString(alex);
        var expectedAgeString = alex.Age.ToString(format);

        fullText
            .Should()
            .Contain(expectedAgeString);
    }

    [Test]
    public void PropertyPrintingConfig_ChangeCultureInfo()
    {
        var dateTimeCulture = personPrintingConfig
            .Printing<DateTime>()
            .Using(CultureInfo.InvariantCulture)
            .PrintToString(alex);
        var dateTimeWithoutCulture = personPrintingConfig
            .PrintToString(alex);
        var expectedAgeString = alex.BirthDate.ToString(CultureInfo.InvariantCulture);

        using (new AssertionScope())
        {
            dateTimeCulture
                .Should()
                .Contain(expectedAgeString);
            dateTimeWithoutCulture
                .Should()
                .NotContain(expectedAgeString);
        }
    }

    [Test]
    public void PrintToString_NotPrintingExcludedPropertyAndField()
    {
        using (new AssertionScope())
        {
            IsContainingProperty(personPrintingConfig.Excluding<Guid>(), alex, nameof(Person.Id))
                .Should()
                .BeFalse();
            IsContainingProperty(personPrintingConfig.Excluding<string>(), alex, nameof(Person.Name))
                .Should()
                .BeFalse();
            IsContainingProperty(personPrintingConfig.Excluding<double>(), alex, nameof(Person.Height))
                .Should()
                .BeFalse();
            IsContainingProperty(personPrintingConfig.Excluding<int>(), alex, nameof(Person.Age))
                .Should()
                .BeFalse();
            IsContainingProperty(personPrintingConfig.Excluding<decimal>(), alex, nameof(Person.BestField))
                .Should()
                .BeFalse();
        }
    }

    private static bool IsContainingProperty(PrintingConfig<Person> printingConfig,
        Person person,
        string propertyName) =>
        printingConfig.PrintToString(person).Contains(propertyName);
}