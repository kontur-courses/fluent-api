using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private static readonly Person Person = new Person
        {
            Age = 19, Height = 175.5, Id = Guid.NewGuid(), Name = "Alex", Wallet = new[] {"$: 50", "P: 40000"},
            RelativesNames = new Dictionary<string, string> {{"Mother", "Lana"}, {"Father", "John"}}
        };

        [Test]
        public void PrintToString_DoNotPrintProperties_AfterExcludingType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>();
            var objInString = printer.PrintToString(Person);
            objInString.Should().NotContain($"{nameof(Person.Name)} = {Person.Name}");
        }

        [Test]
        public void PrintToString_PrintByAnotherMethod_AfterUsing()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(p => p + " y.o.");
            var objInString = printer.PrintToString(Person);
            objInString.Should().Contain($"{nameof(Person.Age)} = {Person.Age} y.o.");
        }

        [Test]
        public void PrintToString_Print_WithConfigCulture()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo("en-US"));
            var objInString = printer.PrintToString(Person);
            objInString.Should().Contain($"{nameof(Person.Height)} = 175.5");
        }

        [Test]
        public void PrintToString_PrintPropByAnotherMethod_AfterPrinting()
        {
            var maxLen = 2;
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(maxLen);
            var objInString = printer.PrintToString(Person);
            objInString.Should().NotContain($"{nameof(Person.Name)} = {Person.Name}");
            objInString.Should().Contain($"{nameof(Person.Name)} = {Person.Name.Substring(0, maxLen)}");
        }

        [Test]
        public void PrintToString_DoNotPrintProperties_AfterExcludingProps()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age);
            var objInString = printer.PrintToString(Person);
            objInString.Should().NotContain($"{nameof(Person.Age)} = {Person.Age}");
        }

        [Test]
        public void PrintToString_PrintCollections_WhenInProperties()
        {
            var printer = ObjectPrinter.For<Person>();
            var objInString = printer.PrintToString(Person);
            objInString.Should().Contain($"{nameof(Person.Wallet)} = [$: 50", "P: 40000]");
            objInString.Should().Contain($"{nameof(Person.RelativesNames)} = [[Mother, Lana], [Father, John]]");
        }
    }
}