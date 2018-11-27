using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
            person = new Person {Name = "Alex", Age = 19, Height = 180 };
        }
        [Test]
        public void Demo()
        {
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<double>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(num => num.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<int>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Age).Using(age => age.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).Trim(10)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Age);

            var s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию 
            var s2 = person.PrintToString();
            //8. ...с конфигурированием
            var s3 = person.PrintToString(x => x.Exclude<double>());

            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        [Test]
        public void ShouldExclude_ByType()
        {
            var printer = ObjectPrinter.For<Person>().Exclude<int>();
            var result = printer.PrintToString(person);
            Console.WriteLine(result);
            Assert.False(result.Contains("Age = 19"));
        }

        [Test]
        public void ShouldExclude_ByProperty()
        {
            var printer = ObjectPrinter.For<Person>().Exclude(p => p.Id);
            var result = printer.PrintToString(person);
            Console.WriteLine(result);
            Assert.False(result.Contains("Id"));
        }

        [Test]
        public void ShouldPrint_UsingTypeFormat()
        {
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(num => "Ы " + num);
            var result = printer.PrintToString(person);
            Console.WriteLine(result);
            Assert.True(result.Contains("Ы"));
        }

        [Test]
        public void ShouldPrint_UsingPropertyFormat()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).Using(n => "ИМЯ: " + n);
            var result = printer.PrintToString(person);
            Console.WriteLine(result);
            Assert.True(result.Contains("ИМЯ"));
        }

        [Test]
        public void ShouldTrim_StringProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name)
                .Using(n => "1234567890")
                .Printing(p => p.Name).Trim(5);
            var result = printer.PrintToString(person);
            Console.WriteLine(result);
            Assert.False(result.Contains("67890"));
        }

        [Test]
        public void ShouldApply_CultureInfo()
        {
            person.Height = 180.2;

            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(new CultureInfo("ru-RU"));
            var result = printer.PrintToString(person);
            Console.WriteLine(result);
            Assert.True(result.Contains("180,2"));

            printer.Printing<double>().Using(new CultureInfo("en-US"));
            result = printer.PrintToString(person);
            Console.WriteLine(result);
            Assert.True(result.Contains("180.2"));
        }

        [Test]
        public void ShouldWork_WithIEnumerable()
        {
            var a = new[] {1, 2, 3};
            Console.WriteLine(a.PrintToString());
        }

        [Test]
        public void DefaultSerialization_ShouldBeEqualTo_ExtensionSerialization()
        {
            var printer = ObjectPrinter.For<Person>();

            Assert.AreEqual(
                printer.PrintToString(person), 
                person.PrintToString());
        }

        [Test]
        public void SerializationWithParams_ShouldBeEqualTo_ExtensionSerializationWithParams()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<double>();

            Assert.AreEqual(
                printer.PrintToString(person),
                person.PrintToString(x => x.Exclude<double>()));
        }

        [Test]
        public void ShouldNotThrowExceptions_When_HasCircularReferences()
        {
            var extendedPerson = new ExtendedPerson() { Age = 20};
            extendedPerson.Parent = extendedPerson;

            Assert.DoesNotThrow(() => extendedPerson.PrintToString());
            Console.WriteLine(extendedPerson.PrintToString());
        }
    }

    class ExtendedPerson : Person
    {
        public Person Parent { get; set; }
    }
}