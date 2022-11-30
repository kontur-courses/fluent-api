namespace ObjectPrintingTests;

[TestFixture]
public class Tests
{
    private Person person;

    [SetUp]
    public void SetUp()
    {
        person = new Person() { Age = 22, Height = 183, Name = "Miposhka", Id = new Guid()};
    }

    [Test]
    public void TestWithoutSettings()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.PrintToString(person);
        Console.Write(s);
    }
    [Test]
    public void TestWithoutNameProperty()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.ExceptProperty(o => o.Name).PrintToString(person);
        Console.Write(s);
    }
    [Test]
    public void TestWithoutStringType()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.ExceptType(typeof(string)).PrintToString(person);
        Console.Write(s);
    }
    [Test]
    public void TestWithChangerOutputNameProperty()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.ChangePropertyOutput(o => o.Name,
                o => "aaa")
            .PrintToString(person);
        Console.Write(s);
    }
    [Test]
    public void TestWithChangerOutputStringTypeProperty()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.ChangeTypeOutput(typeof(string),
                o => "aaa")
            .PrintToString(person);
        Console.Write(s);
    }
    [Test]
    public void TestWithTrimmedString()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.SetStringMaxSize(5).PrintToString(person);
        Console.Write(s);
    }
}