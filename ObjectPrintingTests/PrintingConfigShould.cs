using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework.Internal;
using ObjectPrinting;

namespace ObjectPrintingTests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class PrintingConfigShould
{
    private readonly PrintingConfig<Person> personPrintingConfig = ObjectPrinter.For<Person>();
    private readonly Person alex = new Person(Guid.NewGuid(), "Alex", 188, 18);
    
    [Test]
    public void PrintToString_NotPrintingExcludedProperty()
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
        }
    }

    private static bool IsContainingProperty(PrintingConfig<Person> printingConfig,
        Person person,
        string propertyName) =>
        printingConfig.PrintToString(person).Contains(propertyName);
}