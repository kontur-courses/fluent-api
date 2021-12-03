using System;
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
            person.SetParent(person);
            var result = printer.BuildConfig().PrintToString(person);

            var regex = new Regex("\\s*\\[cyclic reference detected\\]");
            regex.Match(result).Success.Should().BeTrue();
        }

        [Test]
        public void ObjPrinter_ShouldThrowWhenCutNegativeLength()
        {
            Action act = () => printer.PrintingMember(p => p.Name).TrimmedToLength(-1);

            act.Should().Throw<ArgumentException>();
        }     
    }
}