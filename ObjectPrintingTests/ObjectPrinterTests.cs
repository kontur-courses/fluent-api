namespace ObjectPrintingTests;

public class ObjectPrinterTests
{
    [Test]
    public void PrintingConfig_ExcludePropertyType_ShouldExcludeGivenType()
    {
        var person = new Person { Name = "Alex", Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            .ExcludePropertyType<Guid>();

        string s1 = printer.PrintToString(person);
            
        s1.Should().NotContain($"{nameof(person.Id)} = {person.Id}");
    }
    
    [Test]
    public void PrintingConfig_ExcludeProperty_ShouldExcludeGivenProperty()
    {
        var person = new Person { Name = "Alex", Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            .ExcludeProperty(p => p.Name);

        string s1 = printer.PrintToString(person);
            
        s1.Should().NotContain($"{nameof(person.Name)} = {person.Name}");
    }

    [Test]
    public void PrintingConfig_SetPrintingForType_ShouldUseCustomMethod()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        var intValue = "int";

        var printer = ObjectPrinter.For<Person>()
            .SetPrintingFor<int>().Using(p => intValue);

        string s1 = printer.PrintToString(person);
            
        s1.Should().Contain($"{nameof(person.Age)} = {intValue}");
    }

    [Test]
    public void PrintingConfig_SetPrintingForProperty_ShouldUseCustomMethod()
    {
        var person = new Person { Name = "Alex", Age = 19 };
        var idValue = "Id";

        var printer = ObjectPrinter.For<Person>()
            .SetPrintingFor(p => p.Id).Using(prop => idValue);

        string s1 = printer.PrintToString(person);
            
        s1.Should().Contain($"{nameof(person.Id)} = {idValue}");
    }
}