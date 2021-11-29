using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Tests;
using Printer;

namespace PrinterTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;

        private static readonly string[] Cultures =
        {
            "ru", "fi", "ar", "da", "en", "zh", "es", "de", "el"
        };

        [SetUp]
        public void PersonCreator()
        {
            person = new Person
            {
                Name = "Alex",
                Age = 19,
                Height = 164.6,
                Id = Guid.Parse("936DA01F-9ABD-4d9d-80C7-02AF85C822A8"),
                BirthDate = DateTime.Parse("02.11.1996", NumberFormatInfo.InvariantInfo)
            };
        }

        [Test]
        public void Demo()
        {
            person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .StringOf<int>(x => "re");
            printer.PrintToString(person);
            //1. Исключить из сериализации свойства определенного типа
            //2. Указать альтернативный способ сериализации для определенного типа
            //3. Для числовых типов указать культуру
            //4. Настроить сериализацию конкретного свойства
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            //6. Исключить из сериализации конкретного свойства

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }

        [Test]
        public void Exclude_Should_ExcludeExpectedTypes()
        {
            var printingPerson = ObjectPrinter.For<Person>()
                .Exclude<Guid>().PrintToString(person);

            printingPerson.Replace("\r", "").Should().Be("Person\n" +
                                                         "\tName = Alex\n" +
                                                         "\tHeight = 164.6\n" +
                                                         "\tAge = 19\n");
        }

        [Test]
        public void StringOf_Should_BeExpected()
        {
            var expectedSerialisedPerson = "Person\n" +
                                           "\tId = change Guid\n" +
                                           "\tName = change string\n" +
                                           "\tHeight = change double\n" +
                                           "\tAge = change int\n" +
                                           "\tBirthDate = change DateTime\n";
            
            var printer = ObjectPrinter.For<Person>()
                .StringOf<string>(s => "change string")
                .StringOf<int>(i=>"change int")
                .StringOf<DateTime>(d=>"change DateTime")
                .StringOf<double>(d=>"change double")
                .StringOf<Guid>(g=>"change Guid");


            printer.PrintToString(person).Replace("\r","").ToLowerInvariant()
                .Should()
                .Be(expectedSerialisedPerson.ToLowerInvariant());
        }

        [TestCaseSource(nameof(Cultures))]
        public void WithCulture_forGlobalChangeCulture_shouldChangeAllObjects(string cultureTag)
        {
            var culture = new CultureInfo(cultureTag);

            var expectedHeight = person.Height.ToString(culture);
            var expectedBirthDate = person.BirthDate.ToString(culture);
            var expectedSerialisedPerson = "Person\n" +
                                           "\tId = 936DA01F-9ABD-4d9d-80C7-02AF85C822A8\n" +
                                           "\tName = Alex\n" +
                                           $"\tHeight = {expectedHeight}\n" +
                                           "\tAge = 19\n" +
                                           $"\tBirthDate = {expectedBirthDate}\n";

            var actualSerialisedPerson = ObjectPrinter
                .For<Person>()
                .WithCulture(culture)
                .PrintToString(person);

            actualSerialisedPerson.ToLowerInvariant().Replace("\r", "")
                .Should()
                .Be(expectedSerialisedPerson.ToLowerInvariant());
        }

        [TestCaseSource(nameof(Cultures))]
        public void WithCulture_forChangeCultureOnlyBirthDate_shouldNotChangeCultureForAnother(string cultureTag)
        {
            var culture = new CultureInfo(cultureTag);

            var expectedBirthDate = person.BirthDate.ToString(culture);
            var expectedSerialisedPerson = "Person\n" +
                                           "\tId = 936DA01F-9ABD-4d9d-80C7-02AF85C822A8\n" +
                                           "\tName = Alex\n" +
                                           "\tHeight = 164.6\n" +
                                           "\tAge = 19\n" +
                                           $"\tBirthDate = {expectedBirthDate}\n";

            var actualSerialisedPerson = ObjectPrinter
                .For<Person>()
                .WithCultureFor<DateTime>(culture)
                .PrintToString(person);

            actualSerialisedPerson.ToLowerInvariant().Replace("\r", "")
                .Should()
                .Be(expectedSerialisedPerson.ToLowerInvariant());
        }
    }
}