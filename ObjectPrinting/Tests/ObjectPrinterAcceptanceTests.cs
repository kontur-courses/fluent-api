using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person { Name = "Alex", Age = 19, Height = 189.4, Weight = 82.3, Id = Guid.NewGuid() };
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
                .Printing(p => p.Id).Using(i => newPrintingString);

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

            printed.Should().BeEquivalentTo("[1 2 3 4 5]");
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
    }
}