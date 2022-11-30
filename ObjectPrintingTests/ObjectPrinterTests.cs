using System;
using NUnit.Framework;
using ObjectPrinting;
using FluentAssertions;
using System.Globalization;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person { Surname = "Foster", Name = "Alex", Age = 19, Height = 180, Weight = 83.65 };
        }

        [Test]
        public void PrintToString_PrintsDefaultString_WhenNoOptions()
        {
            var actualString = ObjectPrinter.For<Person>().PrintToString(person);

            var expectedSting = $"Person\r\n\tId = Guid\r\n\tSurname = Foster\r\n\tName = Alex\r\n\tAge = 19\r\n\tHeight = 180\r\n\tWeight = 83,65\r\n\tCar = null\r\n";

            expectedSting.Should().Be(actualString);
        }

        [Test]
        public void PrintToString_DontPrintPropertyWithExcludedType()
        {
            var actualString = ObjectPrinter.For<Person>()
               .Excluding<Guid>()
               .PrintToString(person);

            actualString.Should().NotContain("Guid");
        }

        [Test]
        public void PrintToString_DontPrintExcludedProperty()
        {
            var actualString = ObjectPrinter.For<Person>()
               .Excluding(p => p.Height)
               .PrintToString(person);

            actualString.Should().NotContain("Height");
        }

        [Test]
        public void PrintToString_PrintsPropertiesWithTypeSerializationOption()
        {
            var actualString = ObjectPrinter.For<Person>()
                .Printing<string>().Using(i => i.ToUpper())
                .PrintToString(person);

            actualString.Should().Contain(person.Surname.ToUpper()).And.Contain(person.Name.ToUpper());
        }

        [Test]
        public void PrintToString_PrintsNumericalTypeWithSpecifiedCulture()
        {
            var culture = new CultureInfo("en");

            var actualString = ObjectPrinter.For<Person>()
                .Printing<double>().Using(culture)
                .PrintToString(person);

            actualString.Should().Contain($"\tWeight = {person.Weight.ToString(null, culture)}\r\n");
        }

        [Test]
        public void PrintToString_PrintsPropertyWithSerializationOption()
        {
            var actualString = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(age => $"{age} years old")
                .PrintToString(person);

            actualString.Should().Contain($"\tAge = {person.Age} years old\r\n");
        }

        [Test]
        public void PrintToString_PrintsTrimmedStringProperty()
        {
            var maxLength = 2;

            var actualString = ObjectPrinter.For<Person>()
                 .Printing(p => p.Name).TrimmedToLength(maxLength)
                .PrintToString(person);

            actualString.Should().Contain($"\tName = {person.Name.Substring(0, maxLength)}\r\n");
        }

        [Test]
        public void PrintToString_ThrowsInvalidOperationException_WnenCycledReferenceExists()
        {
            var car = new Car { Id = 777 };
            person.Car = car;
            car.Owner = person;

            Action action = () => ObjectPrinter.For<Person>().PrintToString(person);

            action.Should().Throw<InvalidOperationException>();
        }
    }
}
