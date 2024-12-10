using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterTests
    {
        private Person person;

        [SetUp]
        public void Setup()
        {
            person = new Person
                {Name = "Monkey", SecondName = "D.Luffy", NameOfPet = "Usopp", Height = 1234567.89, Age = 17};
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithOutExcludingType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<string>();
            string result = printer.PrintToString(person);
            result.Should().NotContain(person.Name)
                .And.NotContain(person.SecondName)
                .And.NotContain(person.NameOfPet);
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithOutExcludingField()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude(p => p.SecondName);
            string result = printer.PrintToString(person);
            result.Should().NotContain(person.SecondName);
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithSelectedCulture()
        {
            var printer = ObjectPrinter.For<Person>()
                .SelectCulture<double>(new CultureInfo("fr-FR"));
            string result = printer.PrintToString(person);
            result.Should().Contain("1\u00a0234\u00a0567,890");
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithSpecialSerializeType()
        {
            var printer = ObjectPrinter.For<Person>()
                .SerializeTypeWithSpecial<double>((x) => $"~~ {x} ~~");
            string result = printer.PrintToString(person);
            result.Should().Contain("~~ 1234567,89 ~~");
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithSpecialSerializeField()
        {
            var printer = ObjectPrinter.For<Person>()
                .SelectField(p => p.Age).Using(x => 22.ToString());
            string result = printer.PrintToString(person);
            result.Should().Contain("22");
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithTrimmedStringField()
        {
            var printer = ObjectPrinter.For<Person>()
                .SelectField(p => p.SecondName).TrimmedToLength(1);
            string result = printer.PrintToString(person);
            result.Should().Contain("D").And.NotContain("D.Luffy");
        }

        [Test]
        public void ObjetctPrinter_ShouldCorrectPrint_WithExcludingPerson()
        {
            var printer = ObjectPrinter.For<Person>()
                .Exclude<Person>();
            string result = printer.PrintToString(person);
            result.Should().BeEmpty();
        }
    }
}