using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Height = 180.5, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Exclude<Guid>()
                .WithType<double>().SpecificSerialization(x => "")
                .WithType<double>().NumberCulture(CultureInfo.CurrentCulture)
                .WithField(p => p.Age).SpecificSerialization(p => "")
                .WithType<string>().TrimString(6)
                .WithField(p => p.Age).Exclude();

            //1. Исключить из сериализации свойства определенного типа
            //2. Указать альтернативный способ сериализации для определенного типа
            //3. Для числовых типов указать культуру
            //4. Настроить сериализацию конкретного свойства
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            //6. Исключить из сериализации конкретного свойства

            var s1 = printer.PrintToString(person);
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }

        [Test]
        public void Exclude()
        {
            var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Exclude<Guid>();

            printer.PrintToString(actual).Should()
                .Be("Person\n\tName = Alex\n\tHeight = 180,5\n\tAge = 19\n");
        }
        
        [Test]
        public void SpecificTypeSerialization()
        {
            var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .WithType<double>().SpecificSerialization(x => $"double{x}double");

            printer.PrintToString(actual).Should()
                .Be("Person\n\tId = Guid\n\tName = Alex\n\tHeight = double180,5double\n\tAge = 19\n");
        }
        
        [Test]
        public void NumberCulture()
        {
            var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .WithType<double>().NumberCulture(CultureInfo.CurrentCulture);

            printer.PrintToString(actual).Should()
                .Be("Person\n\tId = Guid\n\tName = Alex\n\tHeight = 180,5\n\tAge = 19\n");
        }
        
        [Test]
        public void SpecificFieldSerialization()
        {
            var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .WithField(p => p.Age).SpecificSerialization(p => $"{p}y.o.");

            printer.PrintToString(actual).Should()
                .Be("Person\n\tId = Guid\n\tName = Alex\n\tHeight = 180,5\n\tAge = 19y.o.\n");
        }
        
        [Test]
        public void TrimString()
        {
            var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .WithType<string>().TrimString(6);

            printer.PrintToString(actual).Should()
                .Be("Person");
        }
        
        [Test]
        public void ExcludeField()
        {
            var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .WithField(p => p.Age).Exclude();

            printer.PrintToString(actual).Should()
                .Be("Person\n\tId = Guid\n\tName = Alex\n\tHeight = 180,5\n");
        }
        
        [Test]
        public void MixFilters()
        {
            var actual = new Person { Name = "Alex", Height = 180.5, Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Exclude<Guid>()
                .WithType<double>().SpecificSerialization(x => $"double{x}double")
                .WithType<double>().NumberCulture(CultureInfo.CurrentCulture)
                .WithField(p => p.Age).SpecificSerialization(p => $"{p}y.o.")
                .WithField(p => p.Name).Exclude();

            printer.PrintToString(actual).Should()
                .Be("Person\n\tHeight = double180,5double\n\tAge = 19y.o.\n");
        }
    }
}