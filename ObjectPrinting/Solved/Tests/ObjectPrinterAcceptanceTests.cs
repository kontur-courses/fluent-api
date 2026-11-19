using System;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;
using ObjectPrinting.Solved.Tests.Models;

namespace ObjectPrinting.Solved.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
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
        public void PrintToString_Default_ShouldPrintAllPropertiesValues()
        {
            var person = GetDefaultPerson();

            var result = person.PrintToString();

            var expected = string.Join(Environment.NewLine,
                "Person",
                "\tId = " + person.Id,
                "\tName = " + person.Name,
                "\tLastName = " + person.LastName,
                "\tHeight = " + person.Height,
                "\tAge = " + person.Age,
                "");

            result.Should().Be(expected);
        }


        [Test]
        public void PrintToString_ExcludingType_ShouldExcludeAllSameType()
        {
            var person = GetDefaultPerson();

            var result = person.PrintToString(c => c.Excluding<string>());

            result.Should()
                .NotContainAll("\tName = " + person.Name, "\tLastName = " + person.LastName);
        }

        [Test]
        public void PrintToString_ExcludingProperty_ShouldExcludeProperty()
        {
            var person = GetDefaultPerson();

            var result = person.PrintToString(c => c.Excluding(p => p.Id));

            result.Should()
                .NotContain("\tId = " + person.Id);
        }

        [Test]
        public void PrintToString_ExcludingProperty_ShouldNotExcludeOtherPropertySameType()
        {
            var person = GetDefaultPerson();

            var result = person.PrintToString(c => c.Excluding(p => p.Name));

            result.Should()
                .Contain("\tLastName = " + person.LastName);
        }

        [Test]
        public void PrintToString_PrintingTypeWithUsing_ShouldPrintTypesViaLambda()
        {
            var person = GetDefaultPerson();

            var result = person.PrintToString(
                c => c.Printing<string>().Using(x => x + "wrap by using"));

            result.Should()
                .ContainAll("\tName = " + person.Name + "wrap by using", "\tLastName = " + person.LastName + "wrap by using");
        }

        [Test]
        public void PrintToString_PrintingPropertyWithUsing_ShouldPrintPropertyViaLambda()
        {
            var person = GetDefaultPerson();

            var result = person.PrintToString(
                c => c.Printing(p => p.Id).Using(x => x.ToString("X")));

            result.Should()
                .Contain("\tId = " + person.Id.ToString("X"));
        }

        [TestCase("en-US", "1.85")]
        [TestCase("ru-RU", "1,85")]
        public void PrintToString_PrintingWithCultureInfo_ShouldPrintFormatType(
            string cultureName,
            string expected)
        {
            var person = GetDefaultPerson();
            person.Height = 1.85;

            var result = person.PrintToString(
                c => c.Printing<double>().Using(CultureInfo.GetCultureInfo(cultureName)));

            result.Should().Contain("\tHeight = " + expected);
        }

        [TestCase("Ale", "Ale")]
        [TestCase("Alex", "Alex")]
        [TestCase("Alexa", "Alex")]
        public void PrintToString_PrintingStringTrimmedToLength_ShouldPrintTrimmedString(
            string name, string expectedName)
        {
            var person = GetDefaultPerson();
            person.Name = name;
            const int maxLen = 4;

            var result = person.PrintToString(
                c => c.Printing(p => p.Name).TrimmedToLength(maxLen));

            result.Split(Environment.NewLine).Should()
                .Contain($"\tName = {expectedName}");
        }

        [Test]
        public void PrintToString_WithCircularReferences_ShouldNotThrowStackOverflowException()
        {
            var loopNode = new LoopNode { Value = 1 };
            loopNode.Next = loopNode;

            var action = () => loopNode.PrintToString();

            action.Should().NotThrow<StackOverflowException>();
        }

        [Test]
        public void PrintToString_WithSharedReference_ShouldPrintBothOccurrences()
        {
            var loopNode = new LoopNode
            {
                Value = 1,
                Next = new LoopNode { Value = 2 }
            };
            loopNode.Previous = loopNode.Next;

            var expected = string.Join(Environment.NewLine,
                "LoopNode",
                "\tValue = 1",
                "\tNext = LoopNode",
                "\t\tValue = 2",
                "\t\tNext = null",
                "\t\tPrevious = null",
                "\tPrevious = LoopNode",
                "\t\tValue = 2",
                "\t\tNext = null",
                "\t\tPrevious = null",
                "");

            var result = loopNode.PrintToString();

            result.Should().Be(expected);
        }

        private static Person GetDefaultPerson() => new()
        {
            Name = "Alex",
            LastName = "Smith",
            Age = 19,
            Height = 1.85,
            Id = Guid.NewGuid()
        };
    }
}
