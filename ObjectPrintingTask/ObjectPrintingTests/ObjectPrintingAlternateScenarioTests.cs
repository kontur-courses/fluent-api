using FluentAssertions;
using NUnit.Framework;
using ObjectPrintingTask;
using ObjectPrintingTask.Extensions;
using ObjectPrintingTask.PrintingConfiguration;
using ObjectPrintingTaskTests.TestData;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ObjectPrintingTaskTests
{
    public class ObjectPrintingAlternateScenarioTests
    {
        private readonly Person person = Person.GetTestInstance();
        private Printer<Person> printer;

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
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

        [Test]
        public void ObjPrinter_ShouldUseCultureInfoForMembers()
        {
            printer.PrintingMember(p => p.Height).Using(CultureInfo.GetCultureInfo("RU-ru"));

            var result = printer.PrintToString(person);
            var regex = new Regex("\\s*Height\\s*=\\s*\\d+\\,?\\d*");

            regex.Match(result).Success.Should().BeTrue();
        }

        [Test]
        public void ObjPrinter_ShouldUseCultureInfoForType()
        {
            printer.PrintingType<double>().Using(CultureInfo.GetCultureInfo("RU-ru"));

            var result = printer.PrintToString(person);
            var regex = new Regex("\\s*Height\\s*=\\s*\\d+\\,?\\d*");

            regex.Match(result).Success.Should().BeTrue();
        }

        [Test]
        public void ObjPrinter_ShouldThrowWhenAlternateTypeScenarioIsNull()
        {
            Action action = () => printer.PrintingType<int>().Using(null);

            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ObjPrinter_ShouldThrowWhenAlternateMemberScenarioIsNull()
        {
            Action action = () => printer.PrintingMember(p => p.Name).Using(null);

            action.Should().Throw<ArgumentNullException>();
        }
    }
}
