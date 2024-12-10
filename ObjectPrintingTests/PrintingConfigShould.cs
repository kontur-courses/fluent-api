using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework.Internal;
using ObjectPrinting;

namespace ObjectPrintingTests;

[TestFixture]
//[Parallelizable(ParallelScope.All)]
public class PrintingConfigShould
{
    private readonly PrintingConfig<Person> personPrintingConfig = ObjectPrinter.For<Person>();
    private readonly Person alex = new(Guid.NewGuid(), "Alex", 188, 18);

    [TestCase("X")]
    [TestCase("C")]
    [TestCase("F")]
    public void PropertyPrintingConfig_ChangeSerializationMethod(string format)
    {
        var expectedString = alex.Age.ToString(format);

        personPrintingConfig
            .Printing<int>()
            .Using(i => i.ToString(format))
            .PrintToString(alex)
            .Should()
            .Contain(expectedString);
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