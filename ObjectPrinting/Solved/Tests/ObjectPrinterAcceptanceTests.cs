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
                    person.SecondName,
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
                .NotContainAll(person.Name, person.SecondName);
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
                .Printing(p => p.Id).Using(x => "Айдишник: " + x.ToString("X"))
                .PrintToString(person);

            result.Should()
                .Contain("Айдишник: " + person.Id.ToString("X"));
        }

        private static Person GetDefaultPerson() => new Person
        {
            Name = "Alex",
            SecondName = "Smith",
            Age = 19,
            Height = 1.85,
            Id = Guid.NewGuid()
        };
    }
}