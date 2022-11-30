using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrintingTests.DemoClasses;

namespace ObjectPrintingTests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
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
            yield return new TestCaseData(null, "null").SetName("{m}_Null");
            yield return new TestCaseData(0, "0").SetName("{m}_Int");
            yield return new TestCaseData(0.1, 0.1.ToString(CultureInfo.CurrentCulture)).SetName("{m}_Double");
            yield return new TestCaseData(0.1f, 0.1f.ToString(CultureInfo.CurrentCulture)).SetName("{m}_Float");
            yield return new TestCaseData("string", "string").SetName("{m}_String");
            yield return new TestCaseData(new DateTime(1970, 1, 1),
                new DateTime(1970, 1, 1).ToString(CultureInfo.CurrentCulture)).SetName("{m}_DateTime");
            yield return new TestCaseData(new TimeSpan(0), "00:00:00").SetName("{m}_TimeSpan");
        }
    }

    [TestCaseSource(nameof(SimpleTypesExamples))]
    public void DefaultPrintToString_ReturnCorrectString_OnSimpleType(object value, string expected)
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
    public void DefaultPrintToString_ReturnCorrectString_WithNestedComplexObject()
    {
        var person = new Person { Id = Guid.Empty, Name = "Имя", Height = 0, Age = 0 };
        var personWrapper = new PersonWrapper { Id = 0, Person = person };
        var nl = Environment.NewLine;
        var personExpected = $"Person{nl}\t\tId = Guid{nl}\t\tName = Имя{nl}\t\tHeight = 0{nl}\t\tAge = 0";
        var expected = $"PersonWrapper{nl}\tId = 0{nl}\tPerson = {personExpected}";

        var result = personWrapper.PrintToString();

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

    [Test]
    public void PrintToStringWithConfiguring_ReturnStringEqualsToPrinter()
    {
        var person = new Person { Id = Guid.Empty, Name = "Имя", Height = 0, Age = 0 };
        var printer = ObjectPrinter.For<Person>().Excluding<Guid>();
        var expected = printer.PrintToString(person);

        var result = person.PrintToString(op => op.Excluding<Guid>());

        result.Should().Be(expected);
    }

    [Test]
    public void ExcludingPropertiesByType_WorkCorrectly()
    {
        var person = new Person { Id = Guid.Empty, Name = "Имя", Height = 0, Age = 0 };
        var printer = ObjectPrinter.For<Person>().Excluding<Guid>();
        var nl = Environment.NewLine;
        var expected = $"Person{nl}\tName = Имя{nl}\tHeight = 0{nl}\tAge = 0";

        var result = printer.PrintToString(person);

        result.Should().Be(expected);
    }

    [Test]
    public void ExcludingPropertyByName_WorkCorrectly()
    {
        var person = new Person { Id = Guid.Empty, Name = "Имя", Height = 0, Age = 0 };
        var printer = ObjectPrinter.For<Person>().Excluding(p => p.Id);
        var nl = Environment.NewLine;
        var expected = $"Person{nl}\tName = Имя{nl}\tHeight = 0{nl}\tAge = 0";

        var result = printer.PrintToString(person);

        result.Should().Be(expected);
    }

    [Test]
    public void ExcludingPropertyByName_ThrowException_OnNotPropertyCall()
    {
        var printer = ObjectPrinter.For<Person>();

        var act = () => printer.Excluding(p => 0);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Member selector (p => 0) isn't property call");
    }

    [Test]
    public void PrintingPropertyByType_UsingAlternativePrinting_WorkCorrectly()
    {
        var person = new Person { Id = Guid.Empty, Name = "Имя", Height = 0, Age = 0 };
        var printer = ObjectPrinter.For<Person>()
            .Printing<Guid>().Using(_ => "New Printing");
        var nl = Environment.NewLine;
        var expected = $"Person{nl}\tId = New Printing{nl}\tName = Имя{nl}\tHeight = 0{nl}\tAge = 0";

        var result = printer.PrintToString(person);

        result.Should().Be(expected);
    }

    [Test]
    public void PrintingPropertyByName_UsingAlternativePrinting_WorkCorrectly()
    {
        var person = new Person { Id = Guid.Empty, Name = "Имя", Height = 0, Age = 0 };
        var printer = ObjectPrinter.For<Person>()
            .Printing(p => p.Id).Using(_ => "New Printing");
        var nl = Environment.NewLine;
        var expected = $"Person{nl}\tId = New Printing{nl}\tName = Имя{nl}\tHeight = 0{nl}\tAge = 0";

        var result = printer.PrintToString(person);

        result.Should().Be(expected);
    }

    [Test]
    public void PrintingPropertyByType_UsingAlternativeCulture_WorkCorrectly()
    {
        var person = new Person { Id = Guid.Empty, Name = "Имя", Height = 0.1, Age = 0 };
        var printer = ObjectPrinter.For<Person>()
            .Printing<double>().Using(CultureInfo.InvariantCulture);
        var nl = Environment.NewLine;
        var expected = $"Person{nl}\tId = Guid{nl}\tName = Имя{nl}\tHeight = 0.1{nl}\tAge = 0";

        var result = printer.PrintToString(person);

        result.Should().Be(expected);
    }

    [Test]
    public void PrintingPropertyByType_UsingAlternativeCulture_ChangeCulture()
    {
        const double value = 0.1;
        var ruPrinter = ObjectPrinter.For<double>()
            .Printing<double>().Using(CultureInfo.GetCultureInfo("ru-ru"));
        var invPrinter = ObjectPrinter.For<double>()
            .Printing<double>().Using(CultureInfo.InvariantCulture);

        var ruResult = ruPrinter.PrintToString(value);
        var invResult = invPrinter.PrintToString(value);

        ruResult.Should().NotBe(invResult);
    }

    [TestCase("string", 0, "", TestName = "{m}_OnZeroMaxLength")]
    [TestCase("string", 3, "str", TestName = "{m}_OnMaxLengthLessThenStringLength")]
    [TestCase("string", 6, "string", TestName = "{m}_OnMaxLengthEqualsToStringLength")]
    [TestCase("string", 9, "string", TestName = "{m}_OnMaxLengthMoreThenStringLength")]
    public void PrintingStringProperty_WithTrimmedToLength_WorkCorrectly(string value, int maxLen, string expected)
    {
        var printer = ObjectPrinter.For<string>()
            .Printing<string>().TrimmedToLength(maxLen);

        var ruResult = printer.PrintToString(value);

        ruResult.Should().Be(expected);
    }

    [Test]
    public void PrintingStringProperty_WithTrimmedToLength_ThrowException_OnNegativeMaxLength()
    {
        var printer = ObjectPrinter.For<string>();

        var act = () => printer.Printing<string>().TrimmedToLength(-1);

        act.Should().Throw<ArgumentException>()
            .WithMessage("String length can't be less than zero, but -1");
    }

    [Test]
    public void PrintToString_ThrowException_OnObjectWithCycledReference()
    {
        var obj = new CycledObject();
        var printer = ObjectPrinter.For<CycledObject>();

        var act = () => printer.PrintToString(obj);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Printable object contains circular reference");
    }

    [Test]
    public void PrintToString_ReturnCorrectString_OnArrayOfSimpleObjects()
    {
        var numbers = new[] { 1, 2, 3 };
        var printer = ObjectPrinter.For<int[]>();
        var nl = Environment.NewLine;
        var expected = $"Int32[]{nl}\t[1, 2, 3]";

        var result = printer.PrintToString(numbers);

        result.Should().Be(expected);
    }

    [Test]
    public void PrintToString_ReturnCorrectString_OnArrayOfComplexObjects()
    {
        var numbers = new[] { new Person(), new Person(), new Person() };
        var printer = ObjectPrinter.For<Person[]>();
        var nl = Environment.NewLine;
        var personExpected =
            $"Person{nl}" +
            $"\t\tId = Guid{nl}" +
            $"\t\tName = null{nl}" +
            $"\t\tHeight = 0{nl}" +
            "\t\tAge = 0";
        var expected = $"Person[]{nl}\t[{personExpected},{nl}\t{personExpected},{nl}\t{personExpected}]";

        var result = printer.PrintToString(numbers);

        result.Should().Be(expected);
    }

    [Test]
    public void PrintToString_ReturnCorrectString_OnListOfSimpleObjects()
    {
        var numbers = new List<int> { 1, 2, 3 };
        var printer = ObjectPrinter.For<List<int>>();
        var nl = Environment.NewLine;
        var expected = $"List`1{nl}\t[1, 2, 3]";

        var result = printer.PrintToString(numbers);

        result.Should().Be(expected);
    }

    [Test]
    public void PrintToString_ReturnCorrectString_OnListOfComplexObjects()
    {
        var numbers = new List<Person> { new(), new(), new() };
        var printer = ObjectPrinter.For<List<Person>>();
        var nl = Environment.NewLine;
        var personExpected =
            $"Person{nl}" +
            $"\t\tId = Guid{nl}" +
            $"\t\tName = null{nl}" +
            $"\t\tHeight = 0{nl}" +
            "\t\tAge = 0";
        var expected = $"List`1{nl}\t[{personExpected},{nl}\t{personExpected},{nl}\t{personExpected}]";

        var result = printer.PrintToString(numbers);

        result.Should().Be(expected);
    }

    [Test]
    public void PrintToString_ReturnCorrectString_OnDictionary()
    {
        var numbers = new Dictionary<int, int> { { 1, 2 }, { 2, 3 }, { 3, 4 } };
        var printer = ObjectPrinter.For<Dictionary<int, int>>();
        var nl = Environment.NewLine;
        var kvpExpected = (int k, int v) =>
            $"KeyValuePair`2{nl}" +
            $"\t\tKey = {k}{nl}" +
            $"\t\tValue = {v}";
        var expected = $"Dictionary`2{nl}\t[{kvpExpected(1, 2)},{nl}\t{kvpExpected(2, 3)},{nl}\t{kvpExpected(3, 4)}]";

        var result = printer.PrintToString(numbers);

        result.Should().Be(expected);
    }
    
    [Test]
    public void PrintToString_ReturnCorrectString_OnComplexObjectWithEnumerable()
    {
        var obj = new ObjectWithEnumerable { Enumerable = new []{ 1, 2, 3 }};
        var printer = ObjectPrinter.For<ObjectWithEnumerable>();
        var nl = Environment.NewLine;
        var expected = $"ObjectWithEnumerable{nl}\tEnumerable = Int32[]{nl}\t\t[1, 2, 3]";

        var result = printer.PrintToString(obj);

        result.Should().Be(expected);
    }
    
    [Test]
    public void PrintToString_DontThrowException_OnEqualsObjectOnOneLevel()
    {
        var person = new Person();
        var array = new[] { person, person };
        var printer = ObjectPrinter.For<Person[]>();

        var act = () => printer.PrintToString(array);

        act.Should().NotThrow<InvalidOperationException>();
    }
    
    [Test]
    public void PrintToString_ChangePrinting_ToSimpleTypes()
    {
        const int value = 1;
        var printer = ObjectPrinter.For<int>().Printing<int>().Using(_ => "New printing");
        const string expected = "New printing";

        var result = printer.PrintToString(value);

        result.Should().Be(expected);
    }
}