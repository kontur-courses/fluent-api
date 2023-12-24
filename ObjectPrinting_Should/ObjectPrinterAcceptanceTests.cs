using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrinting_Should
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person = null!;

        [SetUp]
        public void Setup()
        {
            person = new Person { Name = "Alex", Age = 19, Height = 179.5, Id = new Guid() };
        }

        [Test]
        public void Demo()
        {
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Age).Using(i => i.ToString("X"))
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
            var expectedString = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = Alex", "\tHeight = 179,5", "\tAge = 19", "");
        }

        [Test]
        public void PrintToString_SkipsExcludedTypes()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<Guid>();

            var expectedString = string.Join(Environment.NewLine, "Person", "\tName = Alex", "\tHeight = 179,5", "\tAge = 19", "");
            var outputString = printer.PrintToString(person);
            outputString.Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_SkipsExcludedProperty()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Id);

            var expectedString = string.Join(Environment.NewLine, "Person", "\tName = Alex", "\tHeight = 179,5", "\tAge = 19", "");
            var outputString = printer.PrintToString(person);
            outputString.Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_UsesCustomSerializator_WhenGivenToType()
        {
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(i => i.ToString("X"));

            //var change = 19.ToString("X"); //13

            var expectedString = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = Alex", "\tHeight = 179,5", "\tAge = 13", "");
            var outputString = printer.PrintToString(person);
            outputString.Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_UsesCustomSerialization_WhenGivenToProperty()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Age).Using(i => i.ToString("X"));

            //var change = 19.ToString("X"); //13

            var expectedString = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = Alex", "\tHeight = 179,5", "\tAge = 13", "");
            var outputString = printer.PrintToString(person);
            outputString.Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_TrimsStringProperties_WhenTrimmingIsSet()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).TrimmedToLength(1);

            var expectedString = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = A", "\tHeight = 179,5", "\tAge = 19", "");
            var outputString = printer.PrintToString(person);
            outputString.Should().Be(expectedString);
        }

        [Test]
        public void PrintToString_UsesCustomCulture_WhenGivenToNumericType()
        {
            var printer = ObjectPrinter.For<Person>().Printing<double>().Using(CultureInfo.InvariantCulture);

            //var change = 179.5.ToString(CultureInfo.InvariantCulture); 179.5

            var expectedString = string.Join(Environment.NewLine, "Person", "\tId = Guid", "\tName = Alex", "\tHeight = 179.5", "\tAge = 19", "");
            var outputString = printer.PrintToString(person);
            outputString.Should().Be(expectedString);
        }
    }
}