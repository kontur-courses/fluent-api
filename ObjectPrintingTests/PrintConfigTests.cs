using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests
{
    public class PrintConfigTests
    {
        [Test]
        public void PrintToString_ShouldPrintWithoutProperty_WhenItHasExcludedType()
        {
            var person = new Person { Name = "Ivan", Age = 20, Height = 194 };

            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>();
            var actual = printer.PrintToString(person);

            var expected = $"Person\r\n\tName = {person.Name}\r\n\tHeight = {person.Height}\r\n";
            actual.Should().Be(expected);
        }

        [Test]
        public void PrintToString_ShouldPrintWithAlternativeWayOfSerialization_WhenItProvided()
        {
            var person = new Person { Name = "Ivan", Age = 20 };

            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(i => $"_{i}_");
            var actual = printer.PrintToString(person);

            var expected = $"Person\r\n\tName = {person.Name}\r\n\tHeight = {person.Height}\r\n\tAge = _{person.Age}_\r\n";
            actual.Should().Be(expected);
        }
        
        [Test]
        public void PrintToString_ShouldPrintWithAlternativeWayOfCulture_WhenItProvided()
        {
            var person = new Person { Name = "Ivan", Age = 20 };

            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.InvariantCulture);
            var actual = printer.PrintToString(person);

            var expected = $"Person\r\n\tName = {person.Name}\r\n\tHeight = {person.Height.ToString(CultureInfo.InvariantCulture)}\r\n\tAge = 20\r\n";
            actual.Should().Be(expected);
        }
        
        [Test]
        public void PrintToString_ShouldPrintPropertyWithAlternativeWay_WhenItProvided()
        {
            var person = new Person { Name = "Ivan", Age = 20 };

            var printer = ObjectPrinter.For<Person>()
                .Printing(p=> p.Age).Using(x => $"!{x}!");
            var actual = printer.PrintToString(person);

            var expected = $"Person\r\n\tName = {person.Name}\r\n\tHeight = {person.Height}\r\n\tAge = !{person.Age}!\r\n";
            actual.Should().Be(expected);
        }
        
        [Test]
        public void PrintToString_ShouldPrintTrimmedString_WhenItProvided()
        {
            var person = new Person { Name = "Ivan", Age = 20 };

            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().TrimmedToLength(3);
            var actual = printer.PrintToString(person);

            var expected = $"Person\r\n\tName = Iva\r\n\tHeight = {person.Height}\r\n\tAge = {person.Age}\r\n";
            actual.Should().Be(expected);
        }
        
        [Test]
        public void PrintToString_ShouldPrintWithoutProperty_WhenItExcluded()
        {
            var person = new Person { Name = "Ivan", Age = 20 };

            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Name);
            var actual = printer.PrintToString(person);

            var expected = $"Person\r\n\tHeight = {person.Height}\r\n\tAge = {person.Age}\r\n";
            actual.Should().Be(expected);
        }
        
        [Test]
        public void PrintToString_ShouldPrintCycle_WhenCycleReference()
        {
            var classWithCycleReference = new ClassWithCycleReference();
            classWithCycleReference.CycleReference = classWithCycleReference;

            var printer = ObjectPrinter.For<ClassWithCycleReference>()
                .AllowCyclingReference();
            var actual = printer.PrintToString(classWithCycleReference);

            var expected = $"ClassWithCycleReference\r\n\tCycleReference = Cycle\r\n";
            actual.Should().Be(expected);
        }
        
        [Test]
        public void PrintToString_ShouldThrowException_WhenCycleReferenceDoesntSet()
        {
            var classWithCycleReference = new ClassWithCycleReference();
            classWithCycleReference.CycleReference = classWithCycleReference;

            var printer = ObjectPrinter.For<ClassWithCycleReference>();

            FluentActions.Invoking(
                    () => printer.PrintToString(classWithCycleReference))
                .Should().Throw<Exception>()
                .WithMessage("Unexpected cycle reference");
        }
    }
}