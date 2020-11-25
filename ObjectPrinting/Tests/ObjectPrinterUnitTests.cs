using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterUnitTests
    {
        private Person person = new Person(Guid.NewGuid(), "Alex", 182, 24);
        private Point point = new Point(1.2, -5.1, 71.12f);


        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void ExcludingTypes()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>();

            printer.PrintToString(person).Should().Be($"Person{Environment.NewLine}" +
                                                      $"\tId = {person.Id}{Environment.NewLine}" +
                                                      $"\tHeight = {person.Height}{Environment.NewLine}" +
                                                      $"\tAge = {person.Age}{Environment.NewLine}");
        }

        [Test]
        public void ExcludingProperties()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(person => person.Height);

            printer.PrintToString(person).Should().Be($"Person{Environment.NewLine}" +
                                                      $"\tId = {person.Id}{Environment.NewLine}" +
                                                      $"\tName = {person.Name}{Environment.NewLine}" +
                                                      $"\tAge = {person.Age}{Environment.NewLine}");
        }

        [Test]
        public void AlternativeSerializationForSpecificType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(x => (x + 1).ToString());

            printer.PrintToString(person).Should().Be($"Person{Environment.NewLine}" +
                                                      $"\tId = {person.Id}{Environment.NewLine}" +
                                                      $"\tName = {person.Name}{Environment.NewLine}" +
                                                      $"\tHeight = {person.Height}{Environment.NewLine}" +
                                                      $"\tAge = {person.Age + 1}{Environment.NewLine}");
        }

        [Test]
        public void CultureSpecificationForTypes()
        {
            var printer = ObjectPrinter.For<Point>()
                .Printing<float>().UsingCulture(CultureInfo.GetCultureInfo("ru-Ru"));

            Console.WriteLine(printer.PrintToString(point));

            printer.PrintToString(point).Should().Be(
                $"Point{Environment.NewLine}" +
                $"\tX = {point.X}{Environment.NewLine}" +
                $"\tY = {point.Y}{Environment.NewLine}" +
                $"\tZ = {point.Z.ToString(CultureInfo.GetCultureInfo("ru-Ru"))}{Environment.NewLine}");
        }
    }
}