using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person { Name = "Alex", Age = 19, Height = 189.4, Weight = 82.3, Id = Guid.NewGuid() };
        }

        [Test]
        public void PrintToString_ShouldPrintCorrectly()
        {
            var sut = ObjectPrinter.For<Person>();

            var printed = sut.PrintToString(person);

            CheckDefaultPersonPrintingWithoutInnerObjects(printed, person);
        }

        [Test]
        public void PrintToString_ShouldNtPrintProperty_IfTypeExcluded()
        {
            var sut = ObjectPrinter.For<Person>()
                .Excluding<double>();

            var printed = sut.PrintToString(person);

            printed.Should().NotContain("Height").And.NotContain("Weight");
        }

        [Test]
        public void PrintToString_ShouldNtPrintProperty_IfPropertyExcluded()
        {
            var sut = ObjectPrinter.For<Person>()
                .Excluding(p => p.Height);

            var printed = sut.PrintToString(person);

            printed.Should().NotContain("Height").And.Contain("Weight");
        }


        [Test]
        public void PrintToString_ShouldPrintTypeCorrectly_IfSpecifyOtherPrintingMethod()
        {
            var sut = ObjectPrinter.For<Person>()
                .Printing<string>().Using(s => $"string: {s}");

            var printed = sut.PrintToString(person);

            printed.Should().Contain($"Name = string: {person.Name}");
        }

        [Test]
        public void PrintToString_ShouldPrintWithSpecifiedCulture()
        {
            var ruCulture = ObjectPrinter.For<Person>()
                .Printing<double>().Using(new CultureInfo("ru"));
            var enCulture = ObjectPrinter.For<Person>()
                .Printing<double>().Using(new CultureInfo("en"));

            var ruPrinting = ruCulture.PrintToString(person);
            var enPrinting = enCulture.PrintToString(person);

            ruPrinting.Should().Contain($"Height = {person.Height.ToString(new CultureInfo("ru"))}");
            enPrinting.Should().Contain($"Height = {person.Height.ToString(new CultureInfo("en"))}");
        }

        [Test]
        public void PrintToString_ShouldPrintPropertyCorrectly_IfSpecifyOtherPrintingMethod()
        {
            const string newPrintingString = "Id = [HIDDEN]";
            var sut = ObjectPrinter.For<Person>()
                .Printing(p => p.Id).Using(_ => newPrintingString);

            var printed = sut.PrintToString(person);

            printed.Should().Contain(newPrintingString);
        }

        [TestCase("Alex", 3)]
        [TestCase("abracadabra", 4)]
        public void PrintToString_ShouldPrintTrimmedStrings_IfSpecifyTrimming(string original, int trimmingLength)
        {
            person.Name = original;
            person.LastName = "Last Name";
            var sut = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(trimmingLength);

            var printed = sut.PrintToString(person);

            printed.Should().NotContain(original).And.Contain(original[..trimmingLength]).And.Contain("Last Name");
        }

        [Test]
        public void TrimmedToLength_ShouldThrow_IfMaxLengthIsNegative()
        {
            person.Name = "Name";
            person.LastName = "Last Name";
            var sut = ObjectPrinter.For<Person>();

            Action trimming = () => sut.Printing(p => p.Name).TrimmedToLength(-2);

            trimming.Should().Throw<ArgumentException>().WithMessage("Max length should be non-negative");
        }

        [Test]
        public void PrintToString_ShouldNtThrowStackOverflow_IfCircledReferences()
        {
            person.Parent = new Person() { Child = person };
            var sut = ObjectPrinter.For<Person>();

            Action printing = () => sut.PrintToString(person);

            printing.Should().NotThrow();
        }

        [Test]
        public void PrintToString_ShouldPrintAllElementsInArray()
        {
            var arr = new[] { 1, 2, 3, 4, 5 };
            var sut = ObjectPrinter.For<int[]>();

            var printed = sut.PrintToString(arr);

            printed.Should().BeEquivalentTo("[1, 2, 3, 4, 5]");
        }

        [Test]
        public void PrintToString_ShouldPrintAllElementsInArrayOfObjects()
        {
            var secondPerson = new Person();
            var arr = new[] { person, secondPerson };
            var sut = ObjectPrinter.For<Person[]>();

            var printedPersons = sut.PrintToString(arr).Split(", ");

            CheckDefaultPersonPrintingWithoutInnerObjects(printedPersons[0], person);
            CheckDefaultPersonPrintingWithoutInnerObjects(printedPersons[1], secondPerson);
        }

        [Test]
        public void PrintToString_ShouldPrintAllElementsInDictionary()
        {
            var dict = new Dictionary<int, string>()
            {
                { 1, "one" },
                { 2, "two" },
                { 3, "three" },
                { 4, "four" }
            };
            var sut = ObjectPrinter.For<Dictionary<int, string>>();

            var printed = sut.PrintToString(dict);

            printed.Should().Contain("1", "2", "3", "4", "one", "two", "three", "four");
        }

        [Test]
        public void PrintToString_ShouldPrintByLastConfig_IfSeveralConfigsToOneType()
        {
            var sut = ObjectPrinter.For<Person>()
                .Printing<int>().Using(_ => "first config")
                .Printing<int>().Using(_ => "second config");

            var printed = sut.PrintToString(person);

            printed.Should().Contain("second config").And.NotContain("first config");
        }

        [Test]
        public void PrintToString_ShouldPrintByLastConfig_IfSeveralConfigsToOneProperty()
        {
            var sut = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(_ => "first config")
                .Printing(p => p.Age).Using(_ => "second config");

            var printed = sut.PrintToString(person);

            printed.Should().Contain("second config").And.NotContain("first config");
        }

        [Test]
        public void PrintToString_ShouldPrintPropertyByPropertyConfig_IfHasTypeConfigOfProperty()
        {
            var sut = ObjectPrinter.For<Person>()
                .Printing<string>().Using(_ => "first config")
                .Printing(p => p.Name).Using(_ => "second config");

            var printed = sut.PrintToString(person);

            printed.Should().Contain("second config").And.Contain($"LastName = {person.LastName}");
        }

        private void CheckDefaultPersonPrintingWithoutInnerObjects(string printed, Person personToPrint)
        {
            var name = personToPrint.Name ?? "null";
            var lastName = personToPrint.LastName ?? "null";
            printed.Should().Contain(
                $"Person{Environment.NewLine}",
                $"\tId = {personToPrint.Id}{Environment.NewLine}",
                $"\tAge = {personToPrint.Age}{Environment.NewLine}",
                $"\tName = {name}{Environment.NewLine}",
                $"\tLastName = {lastName}{Environment.NewLine}",
                $"\tHeight = {personToPrint.Height}{Environment.NewLine}",
                $"\tWeight = {personToPrint.Weight}{Environment.NewLine}");
        }
    }
}