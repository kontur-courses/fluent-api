namespace ObjectPrintingTests;

[TestFixture]
public class Tests
{
    private Person person;

    [SetUp]
    public void SetUp()
    {
        person = new Person() { Age = 22, Height = 183, Name = "Miposhka", Id = new Guid() };
    }

    [Test]
    public void TestWithoutSettings()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.PrintToString(person);
        s.Should().Be("Person:" + Environment.NewLine + "\tId = 	Guid:" + Environment.NewLine + "\tName = Miposhka" +
                      Environment.NewLine + "\tHeight = 183" + Environment.NewLine + "\tAge = 22" +
                      Environment.NewLine);
    }

    [Test]
    public void TestWithoutNameProperty()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.SelectProperty(o => o.Name).Except().PrintToString(person);
        s.Should().Be("Person:" + Environment.NewLine + "\tId = 	Guid:" + Environment.NewLine + "\tHeight = 183" +
                      Environment.NewLine + "\tAge = 22" + Environment.NewLine);
    }

    [Test]
    public void TestWithoutStringType()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.ExceptType(typeof(string)).PrintToString(person);
        s.Should().Be("Person:" + Environment.NewLine + "\tId = 	Guid:" + Environment.NewLine + "\tHeight = 183" +
                      Environment.NewLine + "\tAge = 22" + Environment.NewLine);
    }

    [Test]
    public void TestWithChangerOutputNameProperty()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer
            .SelectProperty(o => o.Name)
            .ChangeOutput(o => "aaa")
            .PrintToString(person);
        s.Should().Be("Person:" + Environment.NewLine + "\tId = 	Guid:" + Environment.NewLine + "\taaa" +
                      Environment.NewLine + "\tHeight = 183" + Environment.NewLine + "\tAge = 22" +
                      Environment.NewLine);
    }

    [Test]
    public void TestWithChangerOutputStringTypeProperty()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.ChangeTypeOutput(typeof(string),
                o => "aaa")
            .PrintToString(person);
        s.Should().Be("Person:" + Environment.NewLine + "\tId = 	Guid:" + Environment.NewLine + "\taaa" +
                      Environment.NewLine + "\tHeight = 183" + Environment.NewLine + "\tAge = 22" +
                      Environment.NewLine);
    }

    [Test]
    public void TestWithTrimmedString()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.SetStringMaxSize(5).PrintToString(person);
        s.Should().Be("Person:" + Environment.NewLine + "\tId = 	Guid:" + Environment.NewLine + "\tName = Mipos" +
                      Environment.NewLine + "\tHeight = 183" + Environment.NewLine + "\tAge = 22" +
                      Environment.NewLine);
    }
}