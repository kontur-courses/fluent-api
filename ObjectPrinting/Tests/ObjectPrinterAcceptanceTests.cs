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
        public void DemonstatingTest()
        {
            var person = new Person { Name = "12345", Height = 0.1241, Age = 19, Parent = new Person { Name = "Alex2" } };

            var printer = ObjectPrinter
                .For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing(p => p.Name).Using(p => p + "678910")
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(new CultureInfo("ru-RU"))
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);
            
            string s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            Console.WriteLine(person.PrintToString());
            //8. ...с конфигурированием
            Console.WriteLine(person.PrintToString(config => config.Excluding<int>()));
        }

        [Test]
        public void PrintToString_DictionaryTest()
        {
            var dictionary = new Dictionary<int, Person>()
            {
                { 1, new Person() { Name = "Alex1" } },
                { 2, new Person() { Age = 22 } },
                { 3, new Person() { Height = 12.5 } }
            };
            Console.WriteLine(dictionary.PrintToString());
        }
        
        [Test]
        public void PrintToString_ListTest()
        {
            var collection = new List< Person>()
            {
                new() { Name = "Alex1" }, 
                new() { Age = 22 }, 
                new() { Height = 12.5 }
            };
            Console.WriteLine(collection.PrintToString());
        }
        
        [Test]
        public void PrintToString_ArrayTest()
        {
            var collection = new Person[]
            {
                new() { Name = "Alex1" }, 
                new() { Age = 22 }, 
                new() { Height = 12.5 }
            };
            Console.WriteLine(collection.PrintToString());
        }
        
        [Test]
        public void ExcludingPropertyAndTypesTest()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Age).Excluding<string>();

            var actual = printer.PrintToString(person);
            var expected = "Person\r\n\tId = Guid\r\n\tHeight = 0\r\n\tParent = null\r\n";
            actual.Should().Be(expected);
        }

        [Test]
        public void SerializePropertyTest()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Age).Using(p => "Number");

            var actual = printer.PrintToString(person);
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = Number\r\n\tParent = null\r\n";
            actual.Should().Be(expected);
        }
        
        [Test]
        public void SerializeTypeTest()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(p => "Number");

            var actual = printer.PrintToString(person);
            var expected = "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\tHeight = 0\r\n\tAge = Number\r\n\tParent = null\r\n";
            actual.Should().Be(expected);
        }

        [Test]
        public void SetCultureInfoTest()
        {
            var person = new Person { Height = 1.9, Age = 19 };
            var printer = ObjectPrinter
                .For<Person>();
            var ruPrinter = printer.Printing<double>().Using(new CultureInfo("ru-RU"));
            var enPrinter = printer.Printing(p => p.Height).Using(new CultureInfo("en-US"));
            
            var ruActual = ruPrinter.PrintToString(person);
            var enActual = enPrinter.PrintToString(person);
            var ruExpected = "Person\r\n\tId = Guid\r\n\tName = null\r\n\tHeight = 1,9\r\n\tAge = 19\r\n\tParent = null\r\n";
            var enExpected = "Person\r\n\tId = Guid\r\n\tName = null\r\n\tHeight = 1.9\r\n\tAge = 19\r\n\tParent = null\r\n";
            
            ruActual.Should().Be(ruExpected);
            enActual.Should().Be(enExpected);
        }

        [Test]
        public void SeparatePrintersTest()
        {
            var printer = ObjectPrinter
                .For<Person>();
            var withAgePrinter = printer.Printing(p => p.Age).Using(p => "Age");
            var withoutAgePrinter = printer.Printing(p => p.Age).Using(p => "Not Age");

            withAgePrinter.Should().NotBe(withoutAgePrinter);
        }

        [Test]
        public void MultipleSerializeTest()
        {
            var person = new Person { Name = "12345", Parent = new Person { Name = "12345" } };
            var propertyPrinter = ObjectPrinter
                .For<Person>()
                .Printing(p => p.Name).Using(p => p + "678910")
                .Printing(p => p.Name).TrimmedToLength(10);
            
            var typePrinter = ObjectPrinter
                .For<Person>()
                .Printing<string>().Using(p => p + "678910")
                .Printing<string>().TrimmedToLength(10);
            var propertyExpected = "Person\r\n\tId = Guid\r\n\tName = 1234567891\r\n\tHeight = 0\r\n\tAge = 0\r\n\tParent = Person\r\n\t\tId = Guid\r\n\t\tName = 1234567891\r\n\t\tHeight = 0\r\n\t\tAge = 0\r\n\t\tParent = null\r\n";
            var typeExpected = "Person\r\n\tId = Guid\r\n\tName = 1234567891\r\n\tHeight = 0\r\n\tAge = 0\r\n\tParent = Person\r\n\t\tId = Guid\r\n\t\tName = 1234567891\r\n\t\tHeight = 0\r\n\t\tAge = 0\r\n\t\tParent = null\r\n";
            var propertyActual = propertyPrinter.PrintToString(person);
            var typeActual = typePrinter.PrintToString(person);
            
            typeActual.Should().Be(typeExpected);
            propertyActual.Should().Be(propertyExpected);
        }
        
        [Test]
        public void LoopPrintingTest()
        {
            var person = new Person { Name = "Alex"};
            var person2 = new Person { Name = "Alex2", Parent = person };
            person.Parent = person2;

            Console.WriteLine(person.PrintToString());
        }
    }
}