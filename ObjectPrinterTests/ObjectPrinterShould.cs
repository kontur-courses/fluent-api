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
    public class ObjectPrinterShould
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
        public void PrintNull()
        {
            var printer = ObjectPrinter.For<Person>();

            printer.PrintToString(null).Should().Be("null");
        }

        [Test]
        public void ExcludeTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>()
                .Excluding<Guid>();

            printer.PrintToString(person).Should().NotContainAny("Id", "Age");
        }

        [Test]
        public void OverrideTypesPrinting()
        {

            var printer = ObjectPrinter.For<Person>()
                .Printing<Guid>().Using(guid => $"GUID: {guid}")
                .Printing<int>().Using(i => $"int: {i}");

            printer.PrintToString(person).Should()
                .ContainAll($"Id = GUID: {Guid}", "Age = int: 21")
                .And.NotContainAny("Age = 21", $"Id = {Guid}");
        }

        [Test]
        public void OverrideTypesCulture()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(englishCulture)
                .Printing<DateTime>().Using(englishCulture);

            printer.PrintToString(person).Should()
                .ContainAll("Height = 1.92", "Birthday = 1/28/2018")
                .And.NotContain("Height = 1,92");
        }

        [Test]
        public void OverridePropertiesPrinting()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Birthday).Using(d => d.Year.ToString());

            printer.PrintToString(person).Should()
                .Contain("Birthday = 2018")
                .And.NotContainAny(person.Birthday.ToString());
        }

        [Test]
        public void TrimProperties()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(3);

            printer.PrintToString(person).Should()
                .Contain("Name = And")
                .And.NotContain("Andrey");
        }

        [Test]
        public void TrimPropertiesWithShortValues()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(1000);

            printer.PrintToString(person).Should()
                .Contain("Name = Andrey");
        }

        [Test]
        public void ThrowExceptionOnNegativeTrimmingLength()
        {
            Action action = () => ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(-2);

            action.Should().Throw<ArgumentException>()
                .WithMessage("Trimming length can't be negative");
        }

        [Test]
        public void ExcludeProperties()
        {
            var printer = ObjectPrinting.ObjectPrinter.For<Person>()
                .Excluding(p => p.Id)
                .Excluding(p => p.Age);

            printer.PrintToString(person).Should()
                .NotContainAny("Id", "Age");
        }

        [Test]
        public void Demo()
        {
            var printer = ObjectPrinting.ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                
                //3. Для числовых типов указать культуру
                .Printing<DateTime>().Using(englishCulture)

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
            Console.WriteLine(s1);
            //Console.WriteLine(s2);
            //Console.WriteLine(s3);
        }
    }
}