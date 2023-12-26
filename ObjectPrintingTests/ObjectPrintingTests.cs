using System.Globalization;
using ObjectPrinting;


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
            Id = new Guid(),
            Name = "Nikita", 
            Height = 178,
            Age = 20,
            Birthday = new DateTime(2003, 5, 6), 
            Sex = true,
        };
    }

    [Test]
    public void PrintIsUnchanged()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.PrintToString(person);
        s.Should().Be("Person:"+ 
                      "Id = Guid:"+ 
                      "Name = Nikita"+ 
                      "Height = 178"+ 
                      "Age = 20"+ 
                      "Birthday = 06.05.2003 0:00:00"+ 
                      "Sex = True");
    }

    [Test]
    public void ExclusionFieldPropertySerialization()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer
            .Exclude(x => x.Name)
            .Exclude(x => x.Height)
            .PrintToString(person);
        s.Should().Be("Person:"+ 
                      "Id = Guid:"+
                      "Age = 20"+ 
                      "Birthday = 06.05.2003 0:00:00"+ 
                      "Sex = True");
    }

    [Test]
    public void ExclusionFromPropertySerialization()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer
            .Exclude<string>()
            .Exclude<DateTime>()
            .PrintToString(person);
        s.Should().Be("Person:"+ 
                      "Id = Guid:" + 
                      "Height = 178"+ 
                      "Age = 20"+
                      "Sex = True");
    }

    [Test]
    public void ChangingSerializationField()//WithChangerOutputNameProperty()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer
            .SelectProperty(x => x.Name)
            .ChangeField(_ => "Anton")
            .PrintToString(person);
        s.Should().Be("Person:" +
                      "Id = Guid:"+ 
                      "Name = Anton"+ 
                      "Height = 178"+ 
                      "Age = 20"+
                      "Birthday = 06.05.2003 0:00:00"+ 
                      "Sex = True");
    }

    [Test]
    public void ChangingSerializationProperty()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer
            .ChangeProperty<string>(_ => "Egor")
            .PrintToString(person);
        s.Should().Be("Person:"+ 
                      "Id = Guid:"+ 
                      "Name = Egor"+ 
                      "Height = 178"+ 
                      "Age = 20"+ 
                      "Birthday = 06.05.2003 0:00:00"+ 
                      "Sex = True");
    }

    [Test]
    public void TrimmedString()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer.SelectProperty(x => x.Name)
            .SetMaxLength(5)
            .PrintToString(person);
        s.Should().Be("Person:"+ 
                      "Id = Guid:"+ 
                      "Name = Nikit"+ 
                      "Height = 178"+ 
                      "Age = 20"+ 
                      "Birthday = 06.05.2003 0:00:00"+ 
                      "Sex = True");
    }
    [Test]
    public void ChangingCulture()
    {
        var printer = ObjectPrinter.For<Person>();
        var s = printer
            .SelectProperty(x => x.Birthday)
            .ChangeCulture(new CultureInfo("en-US"))
            .PrintToString(person);
        s.Should().Be("Person:"+ 
                      "Id = Guid:"+ 
                      "Name = Nikita"+ 
                      "Height = 178"+ 
                      "Age = 20"+ 
                      "Birthday = 5/6/2003 12:00:00 AM"+ 
                      "Sex = True");
    }
    
}