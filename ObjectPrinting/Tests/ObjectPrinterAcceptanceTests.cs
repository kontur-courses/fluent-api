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
        public void ClassWithCollectionField_Should_BePrintCorrectly()
        {
            var person = new PersonWithCollection
            {
                Name = "Alex",
                Age = 29,
                EmailList = new List<string> {"228@.mail.ru", "myYandex@yandex.ru", "Alex@gmail.com"},
                PhoneNumbers = new List<string>()
            };

            var printer = ObjectPrinter.For<PersonWithCollection>()
                .Printing(p => p.Name).Using(n => n + "IT_IS_ALEX!");
            var result = printer.PrintToString(person);
            Console.WriteLine(result);
            result.Should().Be("PersonWithCollection\r\n" +
                               "\tName = AlexIT_IS_ALEX!\r\n" +
                               "\tAge = 29\r\n" +
                               "\tEmailList = \r\n" +
                               "\t\t228@.mail.ru\r\n" +
                               "\t\tmyYandex@yandex.ru\r\n" +
                               "\t\tAlex@gmail.com\r\n" +
                               "\tPhoneNumbers = Empty collection\r\n");
        }

        [Test]
        public void ClassWithComplexFields_Should_BePrintCorrectly()
        {
            var person = new PersonWithComplexName
            {
                PersonName = new Name {FirstName = "Alex", SecondName = "Obama"},
                Age = 19
            };

            var printer = ObjectPrinter.For<PersonWithComplexName>()
                .Printing(p => p.PersonName.FirstName).TrimToLength(2)
                .Printing(p => p.PersonName.SecondName).TrimToLength(4);
            var result = printer.PrintToString(person);
            Console.WriteLine(result);
            result.Should().Be
            ("PersonWithComplexName\r\n" +
             "\tPersonName = Name\r\n" +
             "\t\tFirstName = Al\r\n" +
             "\t\tSecondName = Obam\r\n" +
             "\tAge = 19\r\n"
            );
        }

        [Test]
        public void ClassWithSimpleFields_Should_BePrintCorrectly()
        {
            var person = new Person
            {
                Name = "     Alex",
                Age = 19,
                Height = 0.3,
                Surname = "Bush",
                Email = "some@mail.ru"
            };

            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<int>().Using(k => k + " 229")
                .Printing<int>().Using(CultureInfo.CurrentCulture)
                .Printing(p => p.Name).Using(s => s.Trim())
                .Printing(p => p.Name).TrimToLength(2)
                .Excluding(p => p.Surname)
                .Printing(p => p.Email).TrimToLength(4);
            var printer1 = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<int>().Using(k => k.ToString())
                .Printing<int>().Using(CultureInfo.CurrentCulture)
                .Printing(p => p.Name).Using(s => s.Trim())
                .Printing(p => p.Name).TrimToLength(2);
            var result = printer.PrintToString(person);
            Console.WriteLine(printer1.PrintToString(person));
            result.Should().Be
            ("Person\r\n" +
             "\tName = Al\r\n" +
             "\tHeight = 0,3\r\n" +
             "\tAge = 19 229\r\n" +
             "\tEmail = some\r\n"
            );
        }

        [Test]
        public void CollectionWithComplexElements_Should_BePrintedCorrectly()
        {
            var testList = new List<Person> {new Person {Name = "olga"}, new Person {Name = "alex"}};
            var printer = ObjectPrinter.For<List<Person>>();
            var result = printer.PrintToString(testList);
            Console.WriteLine(result);
            result.Should().Be("List`1\r\n" +
                               "\tPerson\r\n" +
                               "\t\tId = Guid\r\n" +
                               "\t\tName = olga\r\n" +
                               "\t\tHeight = 0\r\n" +
                               "\t\tAge = 0\r\n" +
                               "\t\tSurname = null\r\n" +
                               "\t\tEmail = null\r\n" +
                               "\tPerson\r\n" +
                               "\t\tId = Guid\r\n" +
                               "\t\tName = alex\r\n" +
                               "\t\tHeight = 0\r\n" +
                               "\t\tAge = 0\r\n" +
                               "\t\tSurname = null\r\n" +
                               "\t\tEmail = null\r\n");
        }
    }
}