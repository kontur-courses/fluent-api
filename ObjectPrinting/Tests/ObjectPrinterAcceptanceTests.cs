using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting.Solved;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {

        private Person testPerson;

        [SetUp]
        public void Setup()
        {
            testPerson = new Person
            {
                Name = "Alexander",
                Age = 25,
                Height = 180.5,
                Id = Guid.NewGuid()
            };
        }

        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 160.1 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .ConfigureType<int>().Using(i => i + " лет");
            //3. Для числовых типов указать культуру
            //.ConfigureType<double>().Using(new CultureInfo("ru-RU"))
            //4. Настроить сериализацию конкретного свойства
            //.ConfigureProperty(p => p.Height).Using(i => i + " см")
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            //.ConfigureProperty(p => p.Name).TrimTo(8)
            //6. Исключить из сериализации конкретного свойства
            //.Exclude(p => p.Name);


            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();

            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Exclude(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            //Console.WriteLine(s3);

        }

        [Test]
        public void PrintToString_DefaultConfiguration_ReturnsAllProperties()
        {
            var result = testPerson.PrintToString();

            result.Should().Contain("Name = Alexander")
                .And.Contain("Age = 25")
                .And.Contain("Height = 180.5")
                .And.Contain("Id = ");
        }

        [Test]
        public void Exclude_ByType_ExcludesPropertiesOfSpecifiedType()
        {
            var printer = ObjectPrinter.For<Person>().Exclude<Guid>();
            var result = printer.PrintToString(testPerson);

            result.Should().NotContain("Id")
                .And.Contain("Name = Alexander")
                .And.Contain("Age = 25")
                .And.Contain("Height = 180.5");
        }
        
        [Test]
        public void Exclude_ByProperty_ExcludesSpecificProperty()
        {
            var printer = new PrintingConfig<Person>().Exclude(p => p.Name);
            var result = printer.PrintToString(testPerson);

            result.Should().NotContain("Name = Alexander")
                .And.Contain("Age = 25")
                .And.Contain("Height = 180.5")
                .And.Contain("Id = ");
        }

        [Test]
        public void Exclude_ByTypeAndProperty_CombinesCorrectly()
        {
            var printer = new PrintingConfig<Person>()
                .Exclude<Guid>()
                .Exclude(p => p.Name);

            var result = printer.PrintToString(testPerson);

            result.Should().NotContain("Id")
                .And.NotContain("Name = Alexander")
                .And.Contain("Age = 25")
                .And.Contain("Height = 180.5");
        }

        [Test]
        public void Exclude_AllPropertiesByCombination_ReturnsOnlyTypeName()
        {
            var printer = new PrintingConfig<Person>()
                .Exclude(p => p.Id)
                .Exclude(p => p.Name)
                .Exclude(p => p.Height)
                .Exclude(p => p.Age);

            var result = printer.PrintToString(testPerson);

            result.Trim().Should().Be("Person");
        }
        
        [Test]
        public void ConfigureType_UsingCustomSerialization_AppliesCustomFormat()
        {
            var printer = new PrintingConfig<Person>()
                .ConfigureType<int>().Using(i => i + " years");

            var result = printer.PrintToString(testPerson);

            result.Should().Contain("Age = 25 years");
        }

        [Test]
        public void ConfigureType_ForStringType_WorksCorrectly()
        {
            var printer = new PrintingConfig<Person>()
                .ConfigureType<string>().Using(s => s?.ToUpper() ?? "NULL");

            var result = printer.PrintToString(testPerson);

            result.Should().Contain("Name = ALEXANDER");
        }

        [Test]
        public void ConfigureType_ForDoubleType_WorksCorrectly()
        {
            var printer = new PrintingConfig<Person>()
                .ConfigureType<double>().Using(d => d + " cm");

            var result = printer.PrintToString(testPerson);

            result.Should().Contain("Height = 180.5 cm");
        }

        [Test]
        public void ConfigureType_WithExclude_WorksTogether()
        {
            var printer = new PrintingConfig<Person>()
                .Exclude<Guid>()
                .ConfigureType<int>().Using(i => i + " лет");

            var result = printer.PrintToString(testPerson);

            result.Should().NotContain("Id")
                .And.Contain("Age = 25 лет");
        }


    }
}