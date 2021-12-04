using System;
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
        private PrintingConfig<Person> printer;

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
        }

        [Test]
        public void ObjPrinter_ShouldPrintNullForNullValues()
        {
            var result = printer.BuildConfig().PrintToString(null);
            result.Should().Contain("null");
        }

        [Test]
        public void ObjPrinter_ShouldGetFullInfo_WhenDefaultConfig()
        {
            var actualResult = printer.BuildConfig().PrintToString(person);

            var regex = new Regex(@"\s*Person\.*Parent\.*Name\.*Surname\.*Height\.*Weight\.*");
            regex.Match(actualResult).Success.Should().BeFalse();
        }

        [Test]
        public void ObjPrinter_ShouldBeReusable()
        {
            printer.BuildConfig().PrintToString(person);
            var result = printer.BuildConfig().PrintToString(person);

            var regex = new Regex("\\s*\\[cyclic reference detected\\]");
            regex.Match(result).Success.Should().BeFalse();
        }

        [Test]
        public void ObjPrinter_ShouldDetectCyclingReference()
        {
            person.Parent = person;
            var result = printer.BuildConfig().PrintToString(person);

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
            var actualResult = printer.BuildConfig().PrintToString(person);

            actualResult.Should().Contain($"Name = {person.Name.Substring(0, cutAmount)}");
        }

        [Test]
        public void ObjPrinter_ShouldCutStringForTypes()
        {
            var cutAmount = 3;
            printer.PrintingType<string>().TrimmedToLength(cutAmount);
            var actualResult = printer.BuildConfig().PrintToString(person);

            actualResult.Should().Contain($"Name = {person.Name.Substring(0, cutAmount)}");
        }

        [Test]
        public void ObjPrinter_ShouldThrowWhenCutNegativeLength()
        {
            Action act = () => printer.PrintingMember(p => p.Name).TrimmedToLength(-1);

            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void ObjPrinter_SholdThrowWhenCutString_StringIsNull()
        {
            var person = new Person();
            var printer = ObjectPrinter.For<Person>().PrintingMember(p => p.Surname).TrimmedToLength(4);
            Action act = () => printer.BuildConfig().PrintToString(person);

            act.Should()
                .Throw<TargetInvocationException>()
                .WithInnerException<ArgumentException>().WithMessage("Cutted string can not be null reference");
        }
    }
}