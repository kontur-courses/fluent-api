using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person {Name = "Alex", Age = 30, Height = 180, Id = new Guid(), Weight = 60};
        }


        [Test]
        public void Demo()
        {
            var person = new Person {Name = "Alex", Age = 19};

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<double>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(prop => prop.ToString())
                //3. Для числовых типов указать культуру
                .Printing<int>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Age).Using(prop => prop.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing<string>().TrimmedToLength(20)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Name);

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
        public void PrinterShouldntPrint_WhenTypeExcluded()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<Guid>();
            printer.PrintToString(person).Should().NotContain("Id = ");
            Console.WriteLine(printer.PrintToString(person));
        }

        [Test]
        public void PrinterShouldntPrint_WhenPropertyExcluded()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Age);
            printer.PrintToString(person).Should().NotContain($"Age = {person.Age}");
            Console.WriteLine(printer.PrintToString(person));
        }

        [Test]
        public void PrinterShouldntPrint_WhenSeveralTypesExcluded()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<Guid>().Excluding<int>();
            printer.PrintToString(person).Should().NotContain("Id = ");
            printer.PrintToString(person).Should().NotContain("Age = ");
        }

        [Test]
        public void PrinterShouldPrintWithFormatting_WhenAlternateSerialization()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(prop => $"{prop} with using");
            printer.PrintToString(person).Should().Contain($"Age = {person.Age} with using");
        }

        [Test]
        public void PrinterShouldPrintWithFormatting_WhenTrimUsed()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().TrimmedToLength(2);
            printer.PrintToString(person).Should().Contain($"Name = {person.Name.Substring(0, 2)}");
        }

        [Test]
        public void PrinterShouldPrintWithFormatting_WhenCultureSet()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(CultureInfo.InvariantCulture);
            printer.PrintToString(person).Should()
                .Contain($"Age = {person.Age.ToString(CultureInfo.InvariantCulture)}");
        }

        [Test]
        public void PrinterShouldWorkCorrect_WhenCircularLinkFound()
        {
            var child = new Person
            {
                Name = "Kate",
                Age = 5,
                Height = 105,
                Id = new Guid(),
                Parent = person
            };
            person.Children.Add(child);

            var printer = ObjectPrinter.For<Person>();
            printer.PrintToString(person).Should().Contain($"Parent = {child.Parent.GetType().Name}");
        }

        [Test]
        public void PrinterShouldWork_WhenObjectHaveEmptyCollection()
        {
            person.PrintToString().Should().Contain("CustomDict = Empty");
            person.PrintToString().Should().Contain("Children = Empty");
        }

        [Test]
        public void PrinterShouldWork_WhenObjectHaveNonEmptyCollection()
        {
            person.CustomDict.Add("string", 1234);
            var serialized = person.PrintToString();
            serialized.Should().Contain($"KeyValuePair`2{Environment.NewLine}");
            serialized.Should().Contain($"Key = string{Environment.NewLine}");
            serialized.Should().Contain($"Value = 1234{Environment.NewLine}");

        }

        [Test]
        public void PrinterShouldWorkCorrect_WhenMultipleConfigurationUsed()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Person>()
                .Printing<int>().Using(prop => "value: " + prop)
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .Printing(p => p.Weight).Using(prop => prop + "kg")
                .Printing<string>().TrimmedToLength(2)
                .Excluding(p => p.Height);

            printer.PrintToString(person).Should().Be(
                $"Person{Environment.NewLine}" +
                $"\tId = Guid{Environment.NewLine}" +
                $"\tName = {person.Name.Substring(0, 2)}{Environment.NewLine}" +
                $"\tAge = value: {person.Age.ToString(CultureInfo.InvariantCulture)}{Environment.NewLine}" +
                $"\tWeight = {person.Weight.ToString(CultureInfo.InvariantCulture)}kg{Environment.NewLine}" +
                $"\tChildren = Empty{Environment.NewLine}" +
                $"\tCustomDict = Empty{Environment.NewLine}");
        }
    }
}