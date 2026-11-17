using System;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;

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

            var result = ObjectPrinter.For<Person>()
                .PrintToString(person);

            result.Should()
                .ContainAll(person.Name,
                    person.Age.ToString(),
                    // ReSharper disable once SpecifyACultureInStringConversionExplicitly
                    person.Height.ToString(),
                    person.Id.ToString());
        }


        [Test]
        public void PrintToString_ExcludingType_ShouldExcludeType()
        {
            var person = GetDefaultPerson();

            var result = ObjectPrinter.For<Person>()
                .Excluding<string>()
                .PrintToString(person);

            result.Should()
                .NotContainAll(person.Name);
        }

        [Test]
        public void PrintToString_ExcludingProperty_ShouldExcludeProperty()
        {
            var person = GetDefaultPerson();

            var result = ObjectPrinter.For<Person>()
                .Excluding(p => p.Id)
                .PrintToString(person);

            result.Should()
                .NotContain(person.Id.ToString());
        }

        [Test]
        public void PrintToString_PrintingTypeWithUsing_ShouldPrintTypeWithUsingLambda()
        {
            var person = GetDefaultPerson();

            var result = ObjectPrinter.For<Person>()
                .Printing<int>().Using(x => x.ToString("B"))
                .PrintToString(person);

            result.Should()
                .Contain(person.Age.ToString("B"));
        }

        [Test]
        public void PrintToString_PrintingPropertyWithUsing_ShouldPrintPropertyWithUsingLambda()
        {
            var person = GetDefaultPerson();

            var result = ObjectPrinter.For<Person>()
                .Printing(p => p.Id).Using(x => x.ToString("X"))
                .PrintToString(person);

            result.Should()
                .Contain(person.Id.ToString("X"));
        }

        [TestCase("en-US", "1.85")]
        [TestCase("ru-RU", "1,85")]
        public void PrintToString_PrintingWithCultureInfo_ShouldPrintFormatType(
            string cultureName,
            string expected)
        {
            var person = GetDefaultPerson();
            person.Height = 1.85;

            var result = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo(cultureName))
                .PrintToString(person);

            result.Should().Contain(expected);
        }

        [TestCase("Alexandra", 4, "Alex")]
        [TestCase("Alex", 4, "Alex")]
        [TestCase("Al", 4, "Al")]
        public void PrintToString_PrintingStringTrimmedToLength_ShouldPrintTrimmedString(
            string name,
            int maxLen,
            string expectedName)
        {
            var person = GetDefaultPerson();
            person.Name = name;

            var result = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(maxLen)
                .PrintToString(person);

            person.Name = expectedName;
            var expected = ObjectPrinter.For<Person>().PrintToString(person);

            result.Should().Be(expected);
        }

        private static Person GetDefaultPerson() => new Person
        {
            Name = "Alex",
            Age = 19,
            Height = 1.85,
            Id = Guid.NewGuid()
        };
    }
}