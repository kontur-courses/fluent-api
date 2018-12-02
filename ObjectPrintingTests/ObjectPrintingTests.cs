using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrintingTests
    {
        private Person person;
        private string printedObject;

        [OneTimeSetUp]
        public void Init()
        {
            person = new Person
            {
                Name = "Alexander",
                Age = 15,
                Height = 5.5
            };
        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine(printedObject);
        }

        [Test]
        public void Should_Exclude_StringMembers()
        {
            var propertyName = nameof(Person.Name);
            printedObject = person.PrintToString(c => c.Excluding<string>());
            person.PrintToString().Should().Contain(propertyName);
            printedObject.Should().NotContain(propertyName);
        }

        [Test]
        public void Should_Exclude_UselessField()
        {
            var propertyName = nameof(person.UselessField);
            person.PrintToString().Should().Contain(propertyName);
            printedObject = person.PrintToString(c => c.Excluding(p => p.UselessField));
            printedObject.Should().NotContain(propertyName);
        }

        [Test]
        public void Should_PrintHex_ForInt()
        {
            person.PrintToString().Should().Contain("Age = 15");
            printedObject = person.PrintToString(c => c.Printing<int>().Using(i => i.ToString("X")));
            printedObject.Should().Contain("Age = F");
        }

        [Test]
        public void Should_PrintHex_ForAge()
        {
            person.PrintToString().Should().Contain("Age = 15");
            printedObject = person.PrintToString(c => c.Printing(p => p.Age).Using(i => i.ToString("X")));
            printedObject.Should().Contain("Age = F");
        }

        [Test]
        public void Should_UseInvariantCulture_ForDouble()
        {
            person.PrintToString().Should().Contain("Height = 5,5");
            printedObject = person.PrintToString(c => c.Printing<double>().Using(CultureInfo.InvariantCulture));
            printedObject.Should().Contain("Height = 5.5");
        }

        [Test]
        public void Should_TrimName_ToFiveChars()
        {
            person.PrintToString().Should().Contain("Alexander");
            printedObject = person.PrintToString(c => c.Printing(p => p.Name).TrimmedToLength(5));
            printedObject.Should().Contain("Name = Alexa");
        }

        [Test]
        public void Should_ApplyConfig_OnElementsOfIEnumerable()
        {
            var printer = ObjectPrinter.For<IEnumerable<double>>().Printing<double>().Using(i => i.ToString("P"));
            printedObject = printer.PrintToString(Enumerable());
            var expected =
                "[0] = 2,00%\r\n\t[1] = 19,00%\r\n\t[2] = 5,60%\r\n\t...\r\n";
            printedObject.Should().Contain(expected);
        }

        [Test]
        public void Should_PrintObjects_WhenTheyAreElementsOfIEnumerable()
        {
            var printer = ObjectPrinter.For<Node[]>();
            var nodes = new[] { new Node(), new Node(), new Node(), new Node() };
            var expected =
                "Node[]\r\n\t[0] = Node\r\n\t\tLeft = null\r\n\t\tRight = null\r\n\t[1] = Node\r\n\t\tLeft = null\r\n\t\tRight = null\r\n\t[2] = Node\r\n\t\tLeft = null\r\n\t\tRight = null\r\n\t...\r\n";
            printedObject = printer.PrintToString(nodes);
            printedObject.Should().Contain(expected);
        }

        private class Node
        {
            public Node Left { get; set; }
            public Node Right { get; set; }
        }

        private IEnumerable<double> Enumerable()
        {
            yield return 0.02;
            yield return 0.19;
            yield return 0.056;
            yield return 0.08;
        }
    }
}