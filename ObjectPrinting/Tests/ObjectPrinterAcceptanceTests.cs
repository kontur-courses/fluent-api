﻿using System;
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
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(i=>i.Height).Using(i => i.ToString("X"))
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
        public void ExcludeFieldType()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>();

            printer.PrintToString(person).ShouldBeEquivalentTo("Person\r\n		Name = Alex\r\n	Height = 0\r\n	Age = 19\r\n");

        }

        [Test]
        public void ExcludeFieldByName()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Id);

            printer.PrintToString(person).ShouldBeEquivalentTo("Person\r\n		Name = Alex\r\n	Height = 0\r\n	Age = 19\r\n");
        }

        [Test]
        public void AlternativePrintingByType()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(x=>"x");

            printer.PrintToString(person).ShouldBeEquivalentTo("Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Height = 0\r\n	Age = x");
        }

        [Test]
        public void AlternativePrintingByPropertyName()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Printing(x=>x.Age).Using(x => "x");

            printer.PrintToString(person).ShouldBeEquivalentTo("Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Height = 0\r\n	Age = x");
        }

        [Test]
        public void AlternativeCultureInfo()
        {
            var person = new Person {Name = "Alex", Height = 1.2};

            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo("en-UK"));

            printer.PrintToString(person).ShouldBeEquivalentTo("Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Height = 1.2	Age = 0\r\n");
        }

        [Test]
        public void TrimmingOfLongStrings()
        {
            var person = new Person { Name = "Alex" };

            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(1);

            printer.PrintToString(person).ShouldBeEquivalentTo("Person\r\n	Id = Guid\r\n	Name = A	Height = 0\r\n	Age = 0\r\n");
        }
    }
}