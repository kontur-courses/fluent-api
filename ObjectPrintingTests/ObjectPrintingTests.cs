using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Configuration;

namespace ObjectPrintingTests;

[TestFixture]
public class ObjectPrintingTests
{
    [Test]
    public void PrintToString_ExcludeTypes_Exclude()
    {
        var person = new Person
        {
            Id = new Guid(),
            Name = "Max",
            Height = 111,
            Age = 25
        };

        var customPrinter = new ObjectPrinter<Person>()
            .Configurate()
            .Excluding<string>()
            .Build();

        var result = customPrinter.PrintToString(person);

        using (new AssertionScope())
        {
            result.Should().NotContain("Name");
            result.Should().NotContain("Max");
        }
    }

    [Test]
    public void PrintToString_ExcludeMember_ShouldExclude()
    {
        var person = new Person
        {
            Id = new Guid(),
            Name = "Max",
            Height = 111,
            Age = 25
        };

        var customPrinter = new ObjectPrinter<Person>()
            .Configurate()
            .Excluding(r => r.Height)
            .Build();

        var result = customPrinter.PrintToString(person);

        using (new AssertionScope())
        {
            result.Should().NotContain("Height");
            result.Should().NotContain("111");
        }
    }


    [Test]
    [TestCase(115.6, "115,6", "Ru-ru", TestName = "Использование культуры Ru-ru для типа double")]
    [TestCase(115.6, "115.6", "En-en", TestName = "Использование культуры En-en для типа double")]
    public void PrintToString_CultureForAlternativeTypeSerialization_RightPrint(double height, string expected, string cultureName)
    {
        var person = new Person
        {
            Id = new Guid(),
            Name = "Max",
            Height = height,
            Age = 25
        };

        var customPrinter = new ObjectPrinter<Person>()
            .Configurate()
            .Printing<double>().Using(CultureInfo.GetCultureInfo(cultureName))
            .Build();

        var result = customPrinter.PrintToString(person);

        result.Should().Contain($"Height = {expected}");
    }

    [Test]
    [TestCase("Ru-ru", TestName = "Использование культуры Ru-ru для поля BirthDate с типом DateTime")]
    [TestCase("En-en", TestName = "Использование культуры En-en для поля BirthDate с типом DateTime")]
    public void PrintToString_CultureForAlternativeMemberSerialization_RightPrint(string cultureName)
    {
        var dateBirth = new DateTime(2000, 01, 01, 15, 45, 00);
        var person = new Person
        {
            Id = new Guid(),
            Name = "Max",
            Height = 115,
            Age = 25,
            BirthDate = dateBirth
        };


        var customPrinter = new ObjectPrinter<Person>()
            .Configurate()
            .Printing(r => r.BirthDate).Using(CultureInfo.GetCultureInfo(cultureName))
            .Build();

        var result = customPrinter.PrintToString(person);

        result.Should().Contain($"BirthDate = {dateBirth.ToString(CultureInfo.GetCultureInfo(cultureName))}");
    }

    [Test]
    public void PrintToString_AlternativeTypeSerialization_RightPrint()
    {
        var person = new Person
        {
            Id = new Guid(),
            Name = "Max",
            Height = 112,
            Age = 25
        };

        var customPrinter = new ObjectPrinter<Person>()
            .Configurate()
            .Printing<double>().Using(r => $"\"type of this member is double and its value {r}\"")
            .Build();

        var result = customPrinter.PrintToString(person);

        result.Should().Contain($"Height = \"type of this member is double and its value {112}\"");
    }

    [Test]
    public void PrintToString_AlternativeMemberSerialization_RightPrint()
    {
        var dateBirth = new DateTime(2000, 01, 01, 15, 45, 00);
        var person = new Person
        {
            Id = new Guid(),
            Name = "Max",
            Height = 115,
            Age = 25,
            BirthDate = dateBirth
        };


        var customPrinter = new ObjectPrinter<Person>()
            .Configurate()
            .Printing(r => r.BirthDate).Using(r => $"\"year: {r.Year}, month: {r.Month}, day: {r.Day}\"")
            .Build();

        var result = customPrinter.PrintToString(person);

        result.Should().Contain($"BirthDate = \"year: {dateBirth.Year}, month: {dateBirth.Month}, day: {dateBirth.Day}\"");
    }

    [TestCase("Max12345678901234567", "Max123456", 9, TestName = "maxLength меньше чем длина")]
    [TestCase("Max12345", "Max12345", 20, TestName = "maxLength больше чем длина, не обрезает строку")]
    public void PrintToString_TrimStringType_RightPrint(string name, string expected, int maxLength)
    {
        var person = new Person
        {
            Name = name,
        };


        var customPrinter = new ObjectPrinter<Person>()
            .Configurate()
            .Printing<string>().TrimmedToLength(maxLength)
            .Build();

        var result = customPrinter.PrintToString(person);

        result.Should().Contain($"Name = {expected}{Environment.NewLine}");
    }

    [Test]
    public void PrintToString_ClassWithChild_RightPrint()
    {
        var parent = new Person
        {
            Name = "parent",
        };

        var person = new Person
        {
            Name = "child",
            Parent = parent
        };


        var customPrinter = new ObjectPrinter<Person>()
            .Configurate()
            .Excluding(r => r.Id)
            .Excluding(r => r.Age)
            .Excluding(r => r.BirthDate)
            .Excluding(r => r.Height)
            .Excluding(r => r.IntField)
            .Build();

        var result = customPrinter.PrintToString(person);
        var expected = $"Person{Environment.NewLine}\tName = child{Environment.NewLine}\tParent = Person{Environment.NewLine}\t\tName = parent{Environment.NewLine}\t\tParent = null{Environment.NewLine}";

        result.Should().Be(expected);
    }

    [Test]
    public void PrintToString_RecycledLinksNotAllowed_ThrowException()
    {
        var person = new Person
        {
            Name = "child",
        };

        person.Parent = person;

        var customPrinter = new ObjectPrinter<Person>()
            .Configurate()
            .Excluding(r => r.Id)
            .Excluding(r => r.Age)
            .Excluding(r => r.BirthDate)
            .Excluding(r => r.Height)
            .Excluding(r => r.IntField)
            .Build();

        new Action(() => { customPrinter.PrintToString(person); }).Should().Throw<Exception>();
    }

    [Test]
    public void PrintToString_RecycledLinksIgnoring_ObjectWithLinkNotPrint()
    {
        var person = new Person
        {
            Name = "child",
        };

        person.Parent = person;

        var customPrinter = new ObjectPrinter<Person>()
            .Configurate()
            .Excluding(r => r.Id)
            .Excluding(r => r.Age)
            .Excluding(r => r.BirthDate)
            .Excluding(r => r.Height)
            .Excluding(r => r.IntField)
            .IgnoringCyclicReferences()
            .Build();

        var result = customPrinter.PrintToString(person);
        result.Should().Be($"Person{Environment.NewLine}\tName = child{Environment.NewLine}\tParent = Person{Environment.NewLine}\t\tcyclic reference{Environment.NewLine}");
    }

    [Test]
    public void PrintToString_ListOfValues_RightPrint()
    {
        var numbers = new List<int>()
        {
            1,
            5,
            10,
            15
        };

        var result = numbers.PrintToString();
        var expected = $"(1, 5, 10, 15){Environment.NewLine}";
        result.Should().Be(expected);
    }

    [Test]
    public void PrintToString_ArrayOfValues_RightPrint()
    {
        var numbers = new uint[]
        {
            0, 15, 2, 155
        };

        var result = numbers.PrintToString();
        var expected = $"[0, 15, 2, 155]{Environment.NewLine}";
        result.Should().Be(expected);
    }


    [Test]
    public void PrintToString_Dictionary_RightPrintValue()
    {
        var numbers = new Dictionary<int, string>
        {
            {
                1, "a"
            },
            {
                2, "b"
            }
        };

        var result = numbers.PrintToString();
        var expected = $"{{{Environment.NewLine}\t{{{Environment.NewLine}\t\tkey: 1{Environment.NewLine}\t\tvalue: a{Environment.NewLine}\t}}," +
            $"{Environment.NewLine}\t{{{Environment.NewLine}\t\tkey: 2{Environment.NewLine}\t\tvalue: b{Environment.NewLine}\t}}{Environment.NewLine}}}{Environment.NewLine}";
        result.Should().Be(expected);
    }


    [Test]
    public void PrintToString_StringBuilder_PrintAsString()
    {
        var sb = new StringBuilder("test123");
        var result = sb.PrintToString();

        var expected = $"test123{Environment.NewLine}";
        result.Should().Be(expected);
    }
}