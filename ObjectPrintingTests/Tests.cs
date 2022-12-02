using System.Globalization;

namespace ObjectPrintingTests;

[TestFixture]
public class Tests
{
    private Person person;

    [SetUp]
    public void SetUp()
    {
        person = new Person
        {
            Age = 22, Height = 183, Name = "Miposhka", Id = new Guid(),
            Birthday = new DateTime(2000, 10, 11), Sex = true,
        };
    }

    [Test]
    public void TestWithoutSettings()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.PrintToString(person);
        s.Should().Be("Person:" + Environment.NewLine + "\tId = Guid:" + Environment.NewLine + "\tName = Miposhka" +
                      Environment.NewLine + "\tHeight = 183" + Environment.NewLine + "\tAge = 22" +
                      Environment.NewLine + "\tBirthday = 11.10.2000 0:00:00" +
                      Environment.NewLine + "\tSex = True" + Environment.NewLine);
    }

    [Test]
    public void TestWithoutNameProperty()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.Except(o => o.Name).PrintToString(person);
        s.Should().Be("Person:" + Environment.NewLine + "\tId = Guid:" + Environment.NewLine + "\tHeight = 183" +
                      Environment.NewLine + "\tAge = 22" + Environment.NewLine + "\tBirthday = 11.10.2000 0:00:00" +
                      Environment.NewLine + "\tSex = True" + Environment.NewLine);
    }

    [Test]
    public void TestWithoutStringType()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.Except<string>().PrintToString(person);
        s.Should().Be("Person:" + Environment.NewLine + "\tId = Guid:" + Environment.NewLine + "\tHeight = 183" +
                      Environment.NewLine + "\tAge = 22" + Environment.NewLine + "\tBirthday = 11.10.2000 0:00:00" +
                      Environment.NewLine + "\tSex = True" + Environment.NewLine);
    }

    [Test]
    public void TestWithChangerOutputNameProperty()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer
            .SelectProperty(o => o.Name)
            .ChangeOutput(o => "aaa")
            .PrintToString(person);
        s.Should().Be("Person:" + Environment.NewLine + "\tId = Guid:" + Environment.NewLine + "\tName = aaa" +
                      Environment.NewLine + "\tHeight = 183" + Environment.NewLine + "\tAge = 22" +
                      Environment.NewLine + "\tBirthday = 11.10.2000 0:00:00" +
                      Environment.NewLine + "\tSex = True" + Environment.NewLine);
    }

    [Test]
    public void TestWithChangerOutputStringTypeProperty()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.ChangeTypeOutput<string>(o => "aaa")
            .PrintToString(person);
        s.Should().Be("Person:" + Environment.NewLine + "\tId = Guid:" + Environment.NewLine + "\tName = aaa" +
                      Environment.NewLine + "\tHeight = 183" + Environment.NewLine + "\tAge = 22" +
                      Environment.NewLine + "\tBirthday = 11.10.2000 0:00:00" +
                      Environment.NewLine + "\tSex = True" + Environment.NewLine);
    }

    [Test]
    public void TestWithTrimmedString()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.SelectProperty(o => o.Name).SetMaxLength(5).PrintToString(person);
        s.Should().Be("Person:" + Environment.NewLine + "\tId = Guid:" + Environment.NewLine + "\tName = Mipos" +
                      Environment.NewLine + "\tHeight = 183" + Environment.NewLine + "\tAge = 22" +
                      Environment.NewLine + "\tBirthday = 11.10.2000 0:00:00" +
                      Environment.NewLine + "\tSex = True" + Environment.NewLine);
    }
    [Test]
    public void ChangeCulture()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.SelectProperty(o => o.Birthday).ChangeCulture(new CultureInfo("en-US")).PrintToString(person);
        s.Should().Be("Person:" + Environment.NewLine + "\tId = Guid:" + Environment.NewLine + "\tName = Miposhka" +
                      Environment.NewLine + "\tHeight = 183" + Environment.NewLine + "\tAge = 22" +
                      Environment.NewLine + "\tBirthday = 10/11/2000 12:00:00 AM" +
                      Environment.NewLine + "\tSex = True" + Environment.NewLine);
    }
}