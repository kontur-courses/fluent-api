namespace ObjectPrintingTests;

public class ObjectPrinterTests
{
    [Test] public void PrintingConfig_ExcludeType_ShouldExcludeGivenType()
    {
        var person = new Person { Name = "Alex", Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            .ExcludePropertyType<Guid>();

        string s1 = printer.PrintToString(person);
            
        s1.Should().NotContain($"{nameof(person.Id)} = {person.Id}");
    }
}