using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var person = new Person{Name = "Vasya", Height = 180};
            var expected = $"\tAge = {person.Age}\r\n\tId = {person.Id}\r\n\tName = {person.Name}\r\n";

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
            var expected = $"\tAge = {person.Age}\r\n\tId = {person.Id}\r\n\tName = {person.Name}\r\n";

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
            var expected = $"\tAge = {person.Age}\r\n\tId = {person.Id}\r\n\tName = {person.Name}\r\nHeight ~ {person.Height}cm";

            var printer = ObjectPrinter
                .For<Person>()
                .Printing(p => p.Height)
                .Using(height => $"Height ~ {height}cm");
            var actually = printer.PrintToString(person);

            actually.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void SetAlternativeSerialization_ForType()
        {
            var person = new Person { Name = "Vasya", Height = 180 };
            var expected = $"\tAge = {person.Age}\r\n\tId = {person.Id}\r\n\tName = {person.Name}\r\nHeight ~ {person.Height}cm";

            var printer = ObjectPrinter
                .For<Person>()
                .Printing<int>()
                .Using(number => $"X");
            var actually = printer.PrintToString(person);

            actually.Should().BeEquivalentTo(expected);
        }
    }

}
