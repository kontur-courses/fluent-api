using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Solved.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void ExcludingType()
        {
            var person = new Person { Name = "Alex", Height = 10, Age = 21 };

            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>();

            var s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
        }

        [Test]
        public void SerializeType()
        {
            var person = new Person { Name = "Alex", Height = 10, Age = 21 };

            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(x => "Число: " + x.ToString());

            var s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
        }

        [Test]
        public void SetCulture()
        {
            var person = new Person { Name = "Alex", Height = 10.5, Age = 21 };

            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.InstalledUICulture);

            var s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
        }

        [Test]
        public void SerializeProperty()
        {
            var person = new Person { Name = "Alex", Height = 10, Age = 21 };

            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(x => $"Возраст: {x}");

            var s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
        }

        [Test]
        public void TrimString()
        {
            var person = new Person { Name = "Alex", Height = 10, Age = 21 };

            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Name).TrimmedToLength(2);

            var s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
        }

        [Test]
        public void ExcludingProperty()
        {
            var person = new Person { Name = "Alex", Height = 10, Age = 21 };

            var printer = ObjectPrinter.For<Person>()
                .Excluding(x => x.Name);

            var s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
        }

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
                .Printing(p => p.Age).Using(x => $"Возраст:{x}")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(2)
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
    }
}