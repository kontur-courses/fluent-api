using FluentAssertions;
using Homework;
using System.Globalization;

namespace HomeworkTests;

[TestFixture]
public class ObjectPrinterTests
{
    [Test]
    public void Demo()
    {
        var person = new Person { Name = "Alex", Age = 19 };

        var printer = ObjectPrinter.For<Person>()
            //1. Исключить из сериализации свойства определенного типа
            .Excluding<Guid>()
            //2. Указать альтернативный способ сериализации для определенного типа
            .Printing<int>().Using(i => i.ToString("X"))
            //3. Для числовых типов указать культуру
            .Printing<double>().Using(CultureInfo.InvariantCulture)
            //4. Настроить сериализацию конкретного свойства
            .Printing(p => p.Weight).Using(t => $"{t} kg")
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            .Printing(p => p.Name).TrimmedToLength(10)
            //6. Исключить из сериализации конкретного свойства
            .Excluding(p => p.Age);

        string s1 = printer.PrintToString(person);

        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
        string s2 = person.PrintToString();

        //8. ...с конфигурированием
        string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
        Console.WriteLine(s1);
        Console.WriteLine(s2);
        Console.WriteLine(s3);
    }

    [Test]
    public void Should_Serialize()
    {
        var person = CreatePerson();
        var printer = ObjectPrinter.For<Person>();

        var actual = printer.PrintToString(person);
        var expected =
           $"""
            Person
            {"\t"}Age = {person.Age}
            {"\t"}Height = {person.Height}
            {"\t"}Id = {person.Id}
            {"\t"}Name = {person.Name}
            {"\t"}Parent = {person.Parent?.ToString() ?? "null"}
            {"\t"}Weight = {person.Weight}

            """;

        actual.Should().Be(expected);
    }

    [Test]
    public void Should_ExcludePropertyOrFieldWithParticularType()
    {
        var person = CreatePerson();
        var printer = ObjectPrinter.For<Person>()
            .Excluding<int>();

        var actual = printer.PrintToString(person);
        var expected =
           $"""
            Person
            {"\t"}Height = {person.Height}
            {"\t"}Id = {person.Id}
            {"\t"}Name = {person.Name}
            {"\t"}Parent = {person.Parent?.ToString() ?? "null"}

            """;

        actual.Should().Be(expected);
    }

    [Test]
    public void Should_UsingAlternativeSerializationForParticularType()
    {
        var person = CreatePerson();
        var printer = ObjectPrinter.For<Person>()
            .Printing<int>().Using(t => $"{t} y.o.");

        var actual = printer.PrintToString(person);
        var expected =
           $"""
            Person
            {"\t"}Age = {person.Age} y.o.
            {"\t"}Height = {person.Height}
            {"\t"}Id = {person.Id}
            {"\t"}Name = {person.Name}
            {"\t"}Parent = {person.Parent?.ToString() ?? "null"}
            {"\t"}Weight = {person.Weight} y.o.

            """;

        actual.Should().Be(expected);
    }

    [Test]
    public void Should_UsingCultureInfo()
    {
        var person = CreatePerson();
        var culture = CultureInfo.GetCultureInfo("ru-RU");
        var printer = ObjectPrinter.For<Person>()
            .Printing<double>().Using(culture);

        var actual = printer.PrintToString(person);
        var expected =
           $"""
         Person
         {"\t"}Age = {person.Age}
         {"\t"}Height = {person.Height.ToString(culture)}
         {"\t"}Id = {person.Id}
         {"\t"}Name = {person.Name}
         {"\t"}Parent = {person.Parent?.ToString() ?? "null"}
         {"\t"}Weight = {person.Weight}

         """;

        actual.Should().Be(expected);
    }

    [Test]
    public void Should_ConfigurePropertyOrField()
    {
        var person = CreatePerson();
        var printer = ObjectPrinter.For<Person>()
            .Printing(t => t.Age).Using(t => $"{t} y.o.")
            .Printing(t => t.Weight).Using(t => $"{t} kg");

        var actual = printer.PrintToString(person);
        var expected =
           $"""
         Person
         {"\t"}Age = {person.Age} y.o.
         {"\t"}Height = {person.Height}
         {"\t"}Id = {person.Id}
         {"\t"}Name = {person.Name}
         {"\t"}Parent = {person.Parent?.ToString() ?? "null"}
         {"\t"}Weight = {person.Weight} kg

         """;

        actual.Should().Be(expected);
    }

    [Test]
    public void Should_TrimString()
    {
        var person = CreatePerson();
        var printer = ObjectPrinter.For<Person>()
            .Printing(t => t.Name).TrimmedToLength(3);

        var actual = printer.PrintToString(person);
        var expected =
           $"""
         Person
         {"\t"}Age = {person.Age}
         {"\t"}Height = {person.Height}
         {"\t"}Id = {person.Id}
         {"\t"}Name = {person.Name[..3]}
         {"\t"}Parent = {person.Parent?.ToString() ?? "null"}
         {"\t"}Weight = {person.Weight}

         """;

        actual.Should().Be(expected);
    }

    [Test]
    public void Should_ExcludeParticularPropertyOrField()
    {
        var person = CreatePerson();
        var printer = ObjectPrinter.For<Person>()
            .Excluding(t => t.Parent);

        var actual = printer.PrintToString(person);
        var expected =
           $"""
         Person
         {"\t"}Age = {person.Age}
         {"\t"}Height = {person.Height}
         {"\t"}Id = {person.Id}
         {"\t"}Name = {person.Name}
         {"\t"}Weight = {person.Weight}

         """;

        actual.Should().Be(expected);
    }

    [Test]
    public void Should_HandlingCircularReferences()
    {
        var person = CreatePerson();
        person.Parent = person;
        var printer = ObjectPrinter.For<Person>();

        var actual = printer.PrintToString(person);
        var expected =
           $"""
        Person
        {"\t"}Age = {person.Age}
        {"\t"}Height = {person.Height}
        {"\t"}Id = {person.Id}
        {"\t"}Name = {person.Name}
        {"\t"}Parent = (cycle with object Person at level 0)
        {"\t"}Weight = {person.Weight}

        """;

        actual.Should().Be(expected);
    }

    [Test]
    public void Should_SerializeCollection()
    {
        var dictionary = new Dictionary<int, int>
        {
            [1] = 2,
            [3] = 4,
            [5] = 6
        };
        var printer = ObjectPrinter.For<Dictionary<int, int>>();

        var actual = printer.PrintToString(dictionary);
        var expected =
           $"""
            [
            {"\t"}[1, 2]
            {"\t"}[3, 4]
            {"\t"}[5, 6]
            ]

            """;

        actual.Should().Be(expected);
    }

    private Person CreatePerson()
    {
        var person = new Person
        {
            Id = Guid.NewGuid(),
            Age = 19,
            Height = 172.5,
            Name = "Name",
            Weight = 75
        };

        return person;
    }
}