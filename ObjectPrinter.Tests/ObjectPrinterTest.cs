using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Tests.Entity;

namespace ObjectPrinting.Tests
{
    public class ObjectPrintingTest
    {
        private Person person;
        [SetUp]
        public void SetUp()
        {
            person = new Person() { Age = 20, Height = 163.21, Name = "Andy" };
        }

        [TestCase(12, ExpectedResult = "12\r\n", TestName = "{m}_int")]
        [TestCase(12.02, ExpectedResult = "12,02\r\n", TestName = "{m}_double")]
        [TestCase(30.6f, ExpectedResult = "30,6\r\n", TestName = "{m}_float")]
        [TestCase("word", ExpectedResult = "word\r\n", TestName = "{m}_string")]
        public string PrintToString_On_SimpleTypes(object obj)
        {
            return obj.PrintToString();
        }

        [Test]
        public void PrintToString_On_Non_NestedClass()
        {
            var result = person.PrintToString();

            result.Should().Be("Person\r\n\tId = Guid\r\n\tName = Andy\r\n\tHeight = 163,21\r\n\tAge = 20\r\n");
        }

        [Test]
        public void PrintToString_On_NestedClass()
        {
            var first = new NestedClass() { Number = 1, Parent = null };
            var second = new NestedClass() { Number = 2, Parent = first };

            var result = second.PrintToString();

            result.Should()
                .Be(
                    "NestedClass\r\n\tNumber = 2\r\n\tParent = NestedClass\r\n\t\t\tNumber = 1\r\n\t\t\tParent = null\r\n");
        }

        [Test]
        public void PrintToString_Should_NotFail_On_NestingLvlGreaterThan10()
        {
            var prev = new NestedClass() { Number = 1, Parent = null };
            var current = new NestedClass() { Number = 2, Parent = prev };
            for (int i = 3; i < 12; i++)
            {
                prev = current;
                current = new NestedClass() { Number = i, Parent = prev };
            }

            Action action = () => current.PrintToString();

            action.Should().NotThrow<StackOverflowException>();
        }

        [Test]
        public void PrintToString_Not_Fail_When_Handling_Circular_References_BetweenObjects()
        {
            var prev = new NestedClass() { Number = 1 };
            var current = new NestedClass() { Number = 2, Parent = prev };
            prev.Parent = current;

            Action action = () => current.PrintToString();

            action.Should().NotThrow<StackOverflowException>();
        }

        [Test]
        public void Excluding_Should_ExcludingType()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<int>();

            var result = printer.PrintToString(person);

            result.Should().Be("Person\r\n\tId = Guid\r\n\tName = Andy\r\n\tHeight = 163,21\r\n");
        }

        [Test]
        public void Excluding_Should_ExcludingPropertyAndField()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p=>p.Id).Excluding(p=>p.Age);

            var result = printer.PrintToString(person);

            result.Should().Be("Person\r\n\tName = Andy\r\n\tHeight = 163,21\r\n");
        }

        [Test]
        public void ObjectPrinter_Should_Configure_Alternative_Serialization_For_Type()
        {
            var printer = ObjectPrinter.For<Person>().Print<int>().As(i => $"{i} y.o");

            var result = printer.PrintToString(person);

            result.Should().Be("Person\r\n\tId = Guid\r\n\tName = Andy\r\n\tHeight = 163,21\r\n\tAge = 20 y.o");
        }

        [Test]
        public void ObjectPrinter_Should_Configure_Alternative_Serialization_For_PropertyOrFiled()
        {
            var printer = ObjectPrinter.For<Person>().
                Print(p=>p.Age).As(i => $"{i} y.o").
                Print(p=>p.Name).As(n=>n+" Warhol");

            var result = printer.PrintToString(person);

            result.Should().Be("Person\r\n\tId = Guid\r\n\tName = Andy Warhol\r\n\tHeight = 163,21\r\n\tAge = 20 y.o\r\n");
        }

        [Test]
        public void ObjectPrinter_Should_SetsCulture_For_IFormattableTypes()
        {
            var printer = ObjectPrinter.For<DateTime>().Print<DateTime>().As(new CultureInfo("en-GB"));
            var dateTime = new DateTime().Date;

            var result = printer.PrintToString(dateTime);

            result.Should().Be(dateTime.ToString(new CultureInfo("en-GB")));
        }

        [Test]
        public void ObjectPrinter_Should_CanCutStrings()
        {
            var printer = ObjectPrinter.For<Person>().Print(p => p.Name).Cut(1);

            var result = printer.PrintToString(person);

            result.Should().Be("Person\r\n\tId = Guid\r\n\tName = A\r\n\tHeight = 163,21\r\n\tAge = 20\r\n");
        }
    }

}