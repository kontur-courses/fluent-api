using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

            var printer = ObjectPrinter.For<Person>();

            string s1 = printer
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<string>().Using(s => s.ToUpper())
                //3. Для числовых типов указать культуру
                .Printing<int>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p=>p.Name).Using(s=>s.ToUpper())
                //5.Настроить обрезание строковых свойств(метод должен быть виден только для строковых свойств)
                .Printing(p=>p.Name).TrimmedToLength(3)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Height)
                .PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию

            //8. ...с конфигурированием
        }

        [Test]
        public void PrintToString_WithoutProperty()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var expected = "Person Id = Guid Name = Alex Height = 0 Age = 19";
            var printer = ObjectPrinter.For<Person>();
            var s = printer.PrintToString(person);
            s.Should().Be(expected);
        }

        [Test]
        public void PrintToString_WithExcludeType()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var expected = "Person Name = Alex Height = 0 Age = 19";
            var printer = ObjectPrinter.For<Person>();
            var s = printer
                .Excluding<Guid>()
                .PrintToString(person);
            s.Should().Be(expected);
        }

        [Test]
        public void PrintToString_WithAlternativeSerializationType()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var expected = "Person Id = Guid Name = ALEX Height = 0 Age = 19";
            var printer = ObjectPrinter.For<Person>();
            var s = printer
                .Printing<string>().Using(s => s.ToUpper())
                .PrintToString(person);
            s.Should().Be(expected);
        }

        [Test]
        public void PrintToString_WithCultureInfo()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.80};
            var expected = "Person Id = Guid Name = Alex Height = 1.8 Age = 19";
            var printer = ObjectPrinter.For<Person>();
            var s = printer
                .Printing<double>().Using(new CultureInfo("en"))
                .PrintToString(person);
            s.Should().Be(expected);
        }

        [Test]
        public void PrintToString_WithAlternativeSerializationProperty()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var expected = "Person Id = Guid Name = ALEX Height = 0 Age = 19";
            var printer = ObjectPrinter.For<Person>();
            var s = printer
                .Printing(p => p.Name).Using(s => s.ToUpper())
                .PrintToString(person);
            s.Should().Be(expected);
        }

        [Test]
        public void PrintToString_WithTrimmedToLength()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var expected = "Person Id = Guid Name = Al Height = 0 Age = 19";
            var printer = ObjectPrinter.For<Person>();
            var s = printer
                .Printing(p => p.Name).TrimmedToLength(2)
                .PrintToString(person);
            s.Should().Be(expected);
        }

        [Test]
        public void PrintToString_WithExcludeProperty()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var expected = "Person Id = Guid Name = Alex Age = 19";
            var printer = ObjectPrinter.For<Person>();
            var s = printer
                .Excluding(p => p.Height)
                .PrintToString(person);
            s.Should().Be(expected);
        }


        [Test]
        public void PrintToString_WithTwoRuleForPropertyString()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var expected = "Person Id = Guid Name = AL Height = 0 Age = 19";
            var printer = ObjectPrinter.For<Person>();
            var s = printer
                .Printing<string>().Using(s => s.ToUpper())
                .Printing(p => p.Name).TrimmedToLength(2)
                .PrintToString(person);
            s.Should().Be(expected);
        }

        [Test]
        public void PrintToString_ForList_WithProperty()
        {
            var persons = new List<Person>
            {
                new Person { Name = "Alex", Age = 19 },
                new Person { Name = "Anna", Age = 16 }
            };

            var expected = "[Person Name = ALEX Age = 19, Person Name = ANNA Age = 16]";
            var printer = ObjectPrinter.For<List<Person>>();
            var s = printer
                .Excluding<Guid>()
                .Excluding<double>()
                .Printing(p => p.FirstOrDefault().Name).Using(s => s.ToUpper())
                .PrintToString(persons);
            s.Should().Be(expected);
        }

        [Test]
        public void PrintToString_ForList_WithoutProperty()
        {
            var list = new List<int> { 1, 2, 3 };
            var expected = "[1, 2, 3]";
            var printer = ObjectPrinter.For<List<int>>();
            var s = printer
                .PrintToString(list);
            s.Should().Be(expected);
        }

        [Test]
        public void PrintToString_ForArray_WithoutProperty()
        {
            var array = new [] { 1, 2, 3 };
            var expected = "[1, 2, 3]";
            var printer = ObjectPrinter.For<int[]>();
            var s = printer
                .PrintToString(array);
            s.Should().Be(expected);
        }

        [Test]
        public void PrintToString_ForDictionary_WithProperty()
        {
            var dict = new Dictionary<int, Person>();
            dict[1] = new Person { Name = "Alex", Age = 19 };
            dict[2] = new Person { Name = "Anna", Age = 16 };

            var expected = "[[1] = Person Name = ALEX Age = 19, [2] = Person Name = ANNA Age = 16]";
            var printer = ObjectPrinter.For<Dictionary<int, Person>>();
            var s = printer
                .Excluding<Guid>()
                .Excluding<double>()
                .Printing(p => p.FirstOrDefault().Value.Name)
                    .Using(s => s.ToUpper())
                .PrintToString(dict);
            s.Should().Be(expected);
        }

    }
}