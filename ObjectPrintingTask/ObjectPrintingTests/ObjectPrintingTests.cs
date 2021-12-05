using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrintingTask;
using ObjectPrintingTask.Extensions;
using ObjectPrintingTask.PrintingConfiguration;
using ObjectPrintingTaskTests.TestData;

namespace ObjectPrintingTaskTests
{
    public class ObjectPrinterTests
    {
        private readonly Person person = Person.GetTestInstance();
        private Printer<Person> printer;

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
        }

        [Test]
        public void ObjPrinter_ShouldPrintNullForNullValues()
        {
            var result = printer.PrintToString(null);
            result.Should().Contain("null");
        }

        [Test]
        public void ObjPrinter_ShouldGetFullInfo_WhenDefaultConfig()
        {
            var actualResult = printer.PrintToString(person);

            var regex = new Regex(@"\s*Person\.*Parent\.*Name\.*Surname\.*Height\.*Weight\.*");
            regex.Match(actualResult).Success.Should().BeFalse();
        }

        [Test]
        public void ObjPrinter_ShouldBeReusable()
        {
            printer.PrintToString(person);
            var result = printer.PrintToString(person);

            var regex = new Regex("\\s*\\[cyclic reference detected\\]");
            regex.Match(result).Success.Should().BeFalse();
        }

        [Test]
        public void ObjPrinter_ShouldDetectCyclingReference()
        {
            person.Parent = person;
            var result = printer.PrintToString(person);

            var regex = new Regex("\\s*\\[cyclic reference detected\\]");
            regex.Match(result).Success.Should().BeTrue();
        }

        [Test]
        public void ObjPrinter_ShouldPrintRepeatedbjects()
        {
            var firstPerson = Person.GetTestInstance();
            var secondPerson = Person.GetTestInstance();
            var thirdPerson = Person.GetTestInstance();

            firstPerson.Family.Add(secondPerson);
            firstPerson.Family.Add(thirdPerson);
            secondPerson.Family.Add(thirdPerson);

            var result = firstPerson.PrintToString();
            result.Should().NotContain("[cyclic reference detected]");
        }

        [Test]
        public void ObjPrinter_ShouldCutStringForMembers()
        {
            var cutAmount = 3;
            printer.PrintingMember(p => p.Name).TrimmedToLength(cutAmount);
            var actualResult = printer.PrintToString(person);

            actualResult.Should().Contain($"Name = {person.Name.Substring(0, cutAmount)}");
        }

        [Test]
        public void ObjPrinter_ShouldCutStringForTypes()
        {
            var cutAmount = 3;
            printer.PrintingType<string>().TrimmedToLength(cutAmount);
            var actualResult = printer.PrintToString(person);

            actualResult.Should().Contain($"Name = {person.Name.Substring(0, cutAmount)}");
        }

        [Test]
        public void ObjPrinter_ShouldThrowWhenCutNegativeLength()
        {
            Action act = () => printer.PrintingMember(p => p.Name).TrimmedToLength(-1);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void ObjPrinter_SholdPrintNull_WhenCuttedStringIsNull()
        {
            var person = new Person();
            var printer = ObjectPrinter.For<Person>().PrintingMember(p => p.Surname).TrimmedToLength(4);
            var result = printer.PrintToString(person);
            result.Should().Contain("Surname = null");
        }

        [TestCase(-1)]
        [TestCase(42)]
        public void ObjPrinter_ShouldPrintIntegers(int number)
        {
            var result = number.PrintToString();

            result.Should().Equals($"{number}");
        }

        [TestCase(42.5)]
        [TestCase(45.0)]
        public void ObjPrinter_ShouldPrintDoubles(double number)
        {
            var result = number.PrintToString();

            result.Should().Equals($"{number}");
        }

        [Test]
        public void ObjPrinter_ShouldUseCultureInfo_ForDouble()
        {
            var number = 42.5;
            var result = number.PrintToString(p => p.PrintingType<double>().Using(CultureInfo.GetCultureInfo("RU-ru")));

            result.Should().Equals("42,5");
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ObjPrinterShouldPrintBooleans(bool value)
        {
            var result = value.PrintToString();

            result.Should().Equals($"{value}");
        }
    }
}