using System;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    public class ObjectPrinterTests
    {
        private Person person;
        private NestedPerson nestedPerson;

        [SetUp]
        public void SetUp()
        {
            person = new Person {Name = "Alex", Age = 19, Height = 2.1};
            nestedPerson = new NestedPerson {Name = "Mike", Age = 99, Height = 2.2, Child = person};
        }

        [Test]
        public void PrintToString_WithoutConfigOneNestingLevel()
        {
            var expected = new ObjectDescription("Person")
                .WithFields(
                    "Id = Guid",
                    "Name = Alex",
                    "Height = 2,1",
                    "Age = 19")
                .ToString();

            var printer = ObjectPrinter.For<Person>();

            printer.PrintToString(person)
                .Should().Be(expected);
        }

        [Test]
        public void PrintToString_WithoutConfigTwoNestingLevels()
        {
            var expected = new ObjectDescription("NestedPerson")
                .WithFields(new ObjectDescription("Child = Person")
                    .WithFields(
                        "Id = Guid",
                        "Name = Alex",
                        "Height = 2,1",
                        "Age = 19"))
                .WithFields(
                    "Id = Guid",
                    "Name = Mike",
                    "Height = 2,2",
                    "Age = 99")
                .ToString();

            var printer = ObjectPrinter.For<NestedPerson>();

            printer.PrintToString(nestedPerson)
                .Should().Be(expected);
        }

        [Test]
        public void PrintToString_Null()
        {
            ObjectPrinter.For<Person>()
                .PrintToString(null)
                .Should().Be("null" + Environment.NewLine);
        }
    }
}