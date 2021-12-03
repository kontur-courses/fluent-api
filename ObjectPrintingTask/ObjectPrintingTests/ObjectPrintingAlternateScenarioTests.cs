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
        private PrintingConfig<Person> printer;

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
        }

        [Test]
        public void ObjPrinter_ShouldUseAlternativeScenario_WithField()
        {
            printer.PrintingMember(p => p.Weight).Using(weight => $"Weight is {weight}");

            var result = printer.BuildConfig().PrintToString(person);

            var regex = new Regex($"\\s*Weight is {person.Weight}");
            regex.Match(result).Success.Should().BeTrue();
        }

        [Test]
        public void ObjPrinter_ShouldUseAlternativeScenario_WithProperty()
        {
            printer.PrintingMember(p => p.Age).Using(a => $"The age is {a}");

            var result = printer.BuildConfig().PrintToString(person);

            var regex = new Regex($"\\s*The age is {person.Age}");
            regex.Match(result).Success.Should().BeTrue();
        }

        [Test]
        public void ObjPrinter_ShouldUseAlternativeScenario_WithType()
        {
            printer.PrintingType<Guid>().Using(g => $"The guid is {g}");

            var result = printer.BuildConfig().PrintToString(person);

            var regex = new Regex($"\\s*The guid is {person.Id}");
            regex.Match(result).Success.Should().BeTrue();
        }

        [Test]
        public void ObjPrinter_ShouldUseCultureInfo()
        {
            printer.PrintingMember(p => p.Height).Using(CultureInfo.GetCultureInfo("RU-ru"));

            var result = printer.BuildConfig().PrintToString(person);
            var regex = new Regex($"\\s*Height\\s*=\\s*\\d+\\,\\d*");

            regex.Match(result).Success.Should().BeTrue();
        }
    }
}
