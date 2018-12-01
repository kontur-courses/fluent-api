using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class PrintingConfig_Should
    {
        [Test]
        public void ExcludeType()
        {
            var person = new Person { Name = "Vasya", Height = 180 };
            var expected = $"\tAge = {person.Age}\r\n\tId = {person.Id}\r\n\tName = {person.Name}\r\n\tNext = null\r\n";

            var printer = ObjectPrinter
                .For<Person>()
                .Excluding<double>();
            var actually = printer.PrintToString(person);

            actually.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ExcludeProperty()
        {
            var person = new Person { Name = "Vasya", Height = 180 };
            var expected =
                $"\tAge = {person.Age}\r\n\tId = {person.Id}\r\n\tName = {person.Name}\r\n\tNext = null\r\n";

            var printer = ObjectPrinter
                .For<Person>()
                .Excluding(property => property.Height);
            var actually = printer.PrintToString(person);

            actually.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void SetAlternativeSerialization_ForProperty()
        {
            var person = new Person { Name = "Vasya", Height = 180 };
            var expected =
                $"\tAge = {person.Age}\r\n\tHeight = {person.Height}cm\r\n\tId = {person.Id}\r\n\tName = {person.Name}\r\n\tNext = null\r\n";

            var printer = ObjectPrinter
                .For<Person>()
                .Printing(p => p.Height)
                .Using(height => $"{height}cm");
            var actually = printer.PrintToString(person);

            actually.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void SetAlternativeSerialization_ForType()
        {
            var person = new Person { Name = "Vasya", Height = 180 };
            var expected =
                $"\tAge = X\r\n\tHeight = {person.Height}\r\n\tId = {person.Id}\r\n\tName = {person.Name}\r\n\tNext = null\r\n";

            var printer = ObjectPrinter
                .For<Person>()
                .Printing<int>()
                .Using(number => "X");
            var actually = printer.PrintToString(person);

            actually.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void SetTrim()
        {
            var person = new Person { Name = "Vasya", Height = 180 };
            var expected =
                $"\tAge = {person.Age}\r\n\tHeight = {person.Height}\r\n\tId = {person.Id}\r\n\tName = V\r\n\tNext = null\r\n";

            var printer = ObjectPrinter
                .For<Person>()
                .Printing(p => p.Name)
                .TrimmedToLength(1);
            var actually = printer.PrintToString(person);

            actually.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void SetCulture()
        {
            var person = new Person { Name = "Vasya", Height = 180.5 };
            var expected =
                $"\tAge = {person.Age}\r\n\tHeight = 180,5\r\n\tId = {person.Id}\r\n\tName = Vasya\r\n\tNext = null\r\n";
            var numberFormat = CultureInfo.GetCultureInfo("es-ES");

            var printer = ObjectPrinter
                .For<Person>()
                .Printing<double>()
                .Using(numberFormat);
            var actually = printer.PrintToString(person);

            actually.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void NotFailOnRecursiveLoop()
        {
            var person = new Person();
            var nextPerson = new Person();
            person.Next = nextPerson;
            nextPerson.Next = person;
            var expected = "A recursive loop was detected.";

            var actual = person.PrintToString();

            actual.Should().Contain(expected);
        }

        [Test]
        public void A()
        {
            var entity = new IEnumerableEntity { Digits = new List<int> { 1, 2, 3 } };
            var expected = "\tDigits = { 1, 2, 3 }";

            var actually = entity.PrintToString();

            actually.Should().BeEquivalentTo(expected);
        }
    }
}
