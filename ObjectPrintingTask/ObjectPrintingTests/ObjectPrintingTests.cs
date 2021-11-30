using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrintingTask;
using ObjectPrintingTask.PrintingConfiguration;
using ObjectPrintingTests.TestData;

namespace ObjectPrintingTests
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
        public void ObjPrinter_ShouldExcludeSpecificType()
        {
            printer.Excluding<Guid>();

            var result = printer.PrintToString(person);

            var regex = new Regex(@"\s*Id\s*=\s*[\d\w\-]+[\d\w]{1}");
            regex.Match(result).Success.Should().BeFalse();
        }

        [Test]
        public void ObjPrinter_ShouldExcludeSpecificProperty()
        {
            printer.Excluding(p => p.Age);

            var result = printer.PrintToString(person);

            var regex = new Regex(@"\s*Age\s*=\s*\d+");
            regex.Match(result).Success.Should().BeFalse();
        }

        [Test]
        public void ObjPrinter_ShouldExcludeSpecificField()
        {
            printer.Excluding(p => p.Weight);

            var result = printer.PrintToString(person);

            var regex = new Regex(@"\s*Weight\s*=\d+}");
            regex.Match(result).Success.Should().BeFalse();
        }

        [Test]
        public void ObjPrinter_ShouldUseAlternativeScenario_WithField()
        {
            printer.PrintingMember(p => p.Weight).Using(weight => $"Weight is {weight}");

            var result = printer.PrintToString(person);

            var regex = new Regex($"\\s*Weight is {person.Weight}");
            regex.Match(result).Success.Should().BeTrue();
        }

        [Test]
        public void ObjPrinter_ShouldUseAlternativeScenario_WithProperty()
        {
            printer.PrintingMember(p => p.Age).Using(a => $"The age is {a}");

            var result = printer.PrintToString(person);

            var regex = new Regex($"\\s*The age is {person.Age}");
            regex.Match(result).Success.Should().BeTrue();
        }

        [Test]
        public void ObjPrinter_ShouldUseAlternativeScenario_WithType()
        {
            printer.PrintingType<Guid>().Using(g => $"The guid is {g}");

            var result = printer.PrintToString(person);

            var regex = new Regex($"\\s*The guid is {person.Id}");
            regex.Match(result).Success.Should().BeTrue();
        }
    }
}