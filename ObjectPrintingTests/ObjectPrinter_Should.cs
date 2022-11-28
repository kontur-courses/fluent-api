using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests;

[TestFixture]
public class ObjectPrinter_Should
{
    [Test]
    public void AcceptanceTest()
    {
        var person = new Person { Name = "Alex", Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            //1. Исключить из сериализации свойства определенного типа
            .Excluding<Guid>()
            //2. Указать альтернативный способ сериализации для определенного типа
            .Printing<int>().Using(i => i.ToString("X"))
            //3. Для всех типов, имеющих культуру, указать культуру
            .Printing<double>().Using(CultureInfo.InvariantCulture)
            //4. Настроить сериализацию конкретного свойства
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            .Printing(p => p.Name).TrimmedToLength(10)
            //6. Исключить из сериализации конкретного свойства
            .Excluding(p => p.Age);

        var s1 = printer.PrintToString(person);

        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
        var s2 = person.PrintToString();

        //8. ...с конфигурированием
        var s3 = person.PrintToString(s => s.Excluding(p => p.Age));

        Console.WriteLine(s1);
        Console.WriteLine(s2);
        Console.WriteLine(s3);
    }

    private static IEnumerable<TestCaseData> SimpleTypesExamples
    {
        get
        {
            yield return new TestCaseData(null, "null").SetName("On null");
            yield return new TestCaseData(0, "0").SetName("On int");
            yield return new TestCaseData(0.1, "0,1").SetName("On double");
            yield return new TestCaseData(0.1f, "0,1").SetName("On float");
            yield return new TestCaseData(new DateTime(1970, 1, 1), "01.01.1970 0:00:00").SetName("On datetime");
            yield return new TestCaseData(new TimeSpan(0), "00:00:00").SetName("On timespan");
        }
    }

    [TestCaseSource(nameof(SimpleTypesExamples))]
    public void DefaultPrintToString_ReturnCorrectString_OnSimpleTypes(object value, string expected)
    {
        var result = value.PrintToString();

        result.Should().Be(expected);
    }

    [Test]
    public void DefaultPrintToString_ReturnCorrectString()
    {
        var person = new Person { Id = Guid.Empty, Name = "Имя", Height = 0, Age = 0 };
        var nl = Environment.NewLine;
        var expected = $"Person{nl}\tId = Guid{nl}\tName = Имя{nl}\tHeight = 0{nl}\tAge = 0";

        var result = person.PrintToString();

        result.Should().Be(expected);
    }

    [Test]
    public void PrintToString_EqualToDefaultPrintToString_OnDefaultPrinter()
    {
        var person = new Person { Id = Guid.Empty, Name = "Имя", Height = 0, Age = 0 };
        var expected = person.PrintToString();
        var printer = ObjectPrinter.For<Person>();

        var result = printer.PrintToString(person);

        result.Should().Be(expected);
    }
}