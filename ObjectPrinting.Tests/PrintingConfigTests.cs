using System.Globalization;
using FluentAssertions;
using ObjectPrinting.Configs;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests;

[TestFixture]
[TestOf(typeof(ObjectPrinter))]
public class PrintingConfigTests
{
    private readonly BasePerson basePerson = BasePerson.Get();
    private PrintingConfig<BasePerson> printingConfig = ObjectPrinter.For<BasePerson>();

    [SetUp]
    public void SetUp()
    {
        printingConfig = ObjectPrinter.For<BasePerson>();
    }

    [Test]
    public void Should_NotThrow_WhenInputNull()
    {
        var config = ObjectPrinter.For<BasePerson>();
        Action action = () => config.PrintToString(null);

        action.Should().NotThrow();
    }

    [Test]
    public void API_Example()
    {
        printingConfig
            //1. Исключить из сериализации свойства определенного типа
            .Excluding<Guid>()
            //2. Указать альтернативный способ сериализации для определенного типа
            .Printing<int>().Using(i => i.ToString("X"))
            //3. Для числовых типов указать культуру
            .Printing<double>().Using(CultureInfo.InvariantCulture)
            //4. Настроить сериализацию конкретного свойства
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            .Printing(p => p.Name).TrimmedToLength(10)
            //6. Исключить из сериализации конкретного свойства
            .Excluding(p => p.Age);

        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
        basePerson.PrintToString();
        //8. ...с конфигурированием
        basePerson.PrintToString(s => s.Excluding(p => p.Age));
    }

    [Test]
    public void Should_UseCultureInfo()
    {
        var culture = CultureInfo.GetCultureInfo("ru-RU");
        printingConfig
            .Printing<double>()
            .Using(culture);

        var actual = printingConfig.PrintToString(basePerson);

        actual.Should().Contain(basePerson.Height.ToString(culture));
    }

    [Test]
    public void Should_TrimString()
    {
        printingConfig
            .Printing<string>()
            .TrimmedToLength(2);
        var actual = printingConfig.PrintToString(basePerson);

        actual.Should().Contain("Name = Pe\r\n");
    }

    [Test]
    public void Should_IgnoreExcludedProperty()
    {
        printingConfig.Excluding(p => p.Array);
        var actual = printingConfig.PrintToString(basePerson);

        actual.Should().NotContain(nameof(basePerson.Array));
    }


    [Test]
    [TestOf(nameof(PrintingConfig<BasePerson>.Excluding))]
    public void Should_IgnoreExcludedType()
    {
        printingConfig.Excluding<Guid>();
        var actual = printingConfig.PrintToString(basePerson);

        actual.Should().NotContain(nameof(basePerson.Id));
    }

    [Test]
    [TestOf(nameof(PrintingConfig<BasePerson>.Excluding))]
    public void Should_NotThrow_WhenContainsCyclicLinks()
    {
        var mock = ExtendedPerson.Get();
        var config = ObjectPrinter.For<ExtendedPerson>();
        Action action = () => config.PrintToString(mock);

        action.Should().NotThrow();
    }

    [Test]
    [TestOf(nameof(PrintingConfig<BasePerson>.Excluding))]
    public void Should_SupportCustomPropertySerializer()
    {
        printingConfig
            .Printing(p => p.Age)
            .Using(v => "123123");

        var actual = printingConfig.PrintToString(basePerson);

        actual.Should().Contain("Age = 123123");
    }

    [Test]
    [TestOf(nameof(PrintingConfig<BasePerson>.Excluding))]
    public void Should_SupportCustomTypeSerializer()
    {
        printingConfig
            .Printing<int>()
            .Using(v => "123123");

        var actual = printingConfig.PrintToString(basePerson);

        actual.Should().Contain("Age = 123123");
    }

    [Test]
    public void Should_SupportArray()
    {
        var array = new object[] { 1, 2, 3 };

        var actual = array.PrintToString();

        var expected = "\r\n[\r\n\t1\r\n\t2\r\n\t3\r\n]";
        actual.Should().Contain(expected);
    }

    [Test]
    public void Should_SupportList()
    {
        var list = new List<int> { 1, 2, 3 };

        var actual = list.PrintToString();

        var expected = "\r\n[\r\n\t1\r\n\t2\r\n\t3\r\n]";
        actual.Should().Contain(expected);
    }

    [Test]
    public void Should_SupportDictionary()
    {
        var dict = new Dictionary<int, string>
        {
            [1] = "123",
            [3] = "345",
        };

        var actual = dict.PrintToString();

        var expected = "{\r\n\t1: 123\r\n\t3: 345\r\n}";

        actual.Should().Contain(expected);
    }
}