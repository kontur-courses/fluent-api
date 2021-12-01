using NUnit.Framework;
using System.Globalization;
using FluentAssertions;
using System;
using System.Linq.Expressions;

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
                .Exclude<string>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString())
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).CropToLength(1)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Age);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }

        [Test]
        public void Exclude_OnPerson_ShouldExclude()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tHeight = 0\r\n\tAge = 19\r\n";
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Exclude<string>();
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void DifferentSerializationForType_OnPerson_ShouldWork()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 190\r\n";
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(x => (((int)x)*10).ToString());
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void DifferentSerializationForProperty_OnPerson_ShouldWork()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 20\r\n";
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Age).Using(x => (((int)x) + 1).ToString());
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void DifferentSerializationForStringProperty_OnPerson_ShouldWork()
        {
            var expected = "Person\r\n\tId = Guid\r\n\tName = Al\r\n\tHeight = 0\r\n\tAge = 19\r\n";
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).CropToLength(2);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ExcludingProperty_OnPerson_ShouldWork()
        {
            var expected = "Person\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = 19\r\n";
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Exclude(p => p.Id);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void AllPossibleParameters_OnPerson_ShouldWork()
        {
            var expected = "Person\r\n\tName = Ale\r\n\tHeight = 100,10223\r\n";
            var person = new Person { Name = "Alex", Age = 10, Height = 100.10223 };
            var printer = ObjectPrinter
                .For<Person>()
                .Exclude(p => p.Id)
                .Printing(p => p.Name)
                .CropToLength(3)
                .Printing<Guid>()
                .Using(x => x.ToString() + " нули хехе")
                .Printing<double>()
                .Using(CultureInfo.InstalledUICulture)
                .Printing<int>()
                .Using(x => ((int)x + 5).ToString())
                .Exclude<int>();

            printer.PrintToString(person).Should().Be(expected);
        }
    }
}