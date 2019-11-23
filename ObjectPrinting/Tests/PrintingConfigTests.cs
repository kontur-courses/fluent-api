using System;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Configs.ConfigInterfaces;

namespace ObjectPrinting.Tests
{
    public class PrintingConfigTests
    {
        private Person person = new Person();

        [SetUp]
        public void SetUp()
        {
            person.Id = Guid.NewGuid();
            person.Name = "Gordon";
            person.Height = 178;
            person.Age = 27;
        }

        [Test]
        public void Excluding_Should_ExcludeCertainFieldsTypes()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<Guid>();
            var result = printer.PrintToString(person);
            result.Should().NotContain("Id");
        }
        
        [Test]
        public void Excluding_Should_ReturnNewPrintingConfigInstance()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<Guid>();
            var printerExcludedInt = printer.Excluding<int>();
            var resultFirst = printer.PrintToString(person);
            var resultSecond = printerExcludedInt.PrintToString(person);
            resultFirst.Should().NotBe(resultSecond, "new instance should be created");
        }

        [Test]
        public void Excluding_Should_ExcludeCertainPropertyFromSerialization()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Age);
            var result = printer.PrintToString(person);
            result.Should().NotContain("Age = 26");
        }
    }
}