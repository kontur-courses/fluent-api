using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Config.Property;
using ObjectPrinting.Config.Type;

namespace ObjectPrinterTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private const string Guid = "0f8fad5b-d9cb-469f-a165-70867728950e";

        private readonly CultureInfo englishCulture =
            CultureInfo.GetCultures(CultureTypes.AllCultures).First(c => c.EnglishName == "English");

        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person
            {
                Name = "Andrey",
                Age = 21,
                Height = 1.92,
                Id = System.Guid.Parse(Guid),
                Birthday = new DateTime(2018, 01, 28)
            };
        }

        [Test]
        public void TestNull()
        {
            var printer = ObjectPrinter.For<Person>();

            printer.PrintToString(null).Should().Be("null");
        }

        [Test]
        public void TestTypeExcluding()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>()
                .Excluding<Guid>();

            printer.PrintToString(person).Should().NotContainAny("Id", "Age");
        }

        [Test]
        public void TestSettingPrintingForType()
        {

            var printer = ObjectPrinter.For<Person>()
                .Printing<Guid>().Using(guid => $"GUID: {guid}")
                .Printing<int>().Using(i => $"int: {i}");

            printer.PrintToString(person).Should()
                .ContainAll($"Id = GUID: {Guid}", "Age = int: 21")
                .And.NotContainAny("Age = 21", $"Id = {Guid}");
        }

        [Test]
        public void TestSettingCultureForType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(englishCulture)
                .Printing<DateTime>().Using(englishCulture);

            printer.PrintToString(person).Should()
                .ContainAll("Height = 1.92", "Birthday = 1/28/2018")
                .And.NotContain("Height = 1,92");
        }

        [Test]
        public void TestSettingPrintingForProperty()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Birthday).Using(d => d.Year.ToString());

            printer.PrintToString(person).Should()
                .Contain("Birthday = 2018")
                .And.NotContainAny(person.Birthday.ToString());
        }

        [Test]
        public void TestSettingTrimming()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(3);

            var result = printer.PrintToString(person);
            Console.WriteLine(result);
            result.Should()
                .Contain("Name = And")
                .And.NotContain("Andrey");
        }

        [Test, Ignore("Not implemented yet")]
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
                .Printing(p => p.Height).Using(i => i.ToString())

               //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)

               //6. Исключить из сериализации конкретного свойства
               .Excluding(p => p.Age);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            //string s2 = person.PrintToString();

            //8. ...с конфигурированием
            //string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            //Console.WriteLine(s1);
            //Console.WriteLine(s2);
            //Console.WriteLine(s3);
        }
    }
}