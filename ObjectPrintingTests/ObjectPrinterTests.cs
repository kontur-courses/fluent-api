using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests
{
    public class ObjectPrinterTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person()
            {
                Id = Guid.Empty,
                Name = "Alex",
                Surname = "Ivanov",
                Height = 185.5,
                Age = 19,
                Money = 300.99,
                Family = new List<Person>()
            };
        }

        [Test]
        public void PrintToString_IsNotPrintingType_WhenTypeIsExcluded()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>();

            var result = printer.PrintToString(person);

            result.Should().NotContain($"{nameof(person.Id)} = {person.Id}");
        }

        [Test]
        public void PrintToString_IsNotPrintingMember_WhenMemberIsExcluded()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => person.Name);

            var result = printer.PrintToString(person);

            result.Should().NotContain($"{nameof(person.Name)} = {person.Name}");
        }

        [Test]
        public void PrintToString_UsesAlternativeTypeSerializer_WhenSerializerDefined()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().Using(x => x.ToUpper());

            var result = printer.PrintToString(person);

            result.Should().ContainAll($"{nameof(person.Name)} = {person.Name.ToUpper()}",
                $"{nameof(person.Surname)} = {person.Surname.ToUpper()}");
        }

        [Test]
        public void PrintToString_UsesAlternativeMemberSerializer_WhenSerializerDefined()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => person.Money).Using(d => $"{d} dollars");

            var result = printer.PrintToString(person);

            result.Should().Contain($"{nameof(person.Money)} = {person.Money} dollars");
        }

        [Test]
        public void PrintToString_UsesCultureForFormattableTypes_WhenCultureDefined()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.GetCultureInfo("ru-RU"));

            var result = printer.PrintToString(person);

            result.Should().ContainAll($"{nameof(person.Height)} = 185,5",
                $"{nameof(person.Money)} = 300,99");
        }

        [Test]
        public void PrintToString_TrimsStringProperties_WhenTrimmedToLengthUsed()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(p => person.Surname).TrimmedToLength(4);

            var result = printer.PrintToString(person);

            result.Should().Contain($"{nameof(person.Surname)} = {person.Surname.Substring(0, 4)}");
        }

        [Test]
        public void PrintToString_ThrowsOnTrimmedLength_WhenMaxLengthIsNegative()
        {
            Action action = () => ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(-2);

            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void PrintToString_PrintsEnumerableCorrectly_WhenPrintingList()
        {
            var list = new List<string>() { "a", "b", "c" };
            var printer = ObjectPrinter.For<List<string>>();

            var result = printer.PrintToString(list);

            result.Should().ContainAll("a", "b", "c");
        }

        [Test]
        public void PrintToString_PrintsEnumerableCorrectly_WhenPrintingArray()
        {
            var array = new int[] { 1, 2, 3 };
            var printer = ObjectPrinter.For<int[]>();

            var result = printer.PrintToString(array);

            result.Should().ContainAll("1", "2", "3");
        }

        [Test]
        public void PrintToString_PrintsEnumerableCorrectly_WhenPrintingDictionary()
        {
            var array = new Dictionary<int, string>() { { 1, "Alex" }, { 2, "Sam" }, { 3, "Dan" } };
            var printer = ObjectPrinter.For<Dictionary<int, string>>();

            var result = printer.PrintToString(array);

            result.Should().ContainAll("Key = 1", "Value = Alex", "Key = 2", "Value = Sam", "Key = 3", "Value = Dan");
        }

        [Test]
        public void PrintToString_PrintsEnumerableCorrectly_WhenEnumerableEmpty()
        {
            var printer = ObjectPrinter.For<Person>();

            var result = printer.PrintToString(person);

            result.Should().Contain($"{nameof(person.Family)} = Empty collection");
        }

        [Test]
        public void PrintToString_CorrectlyProcessingCyclicReferences_WhenTheyAppear()
        {
            var newPerson = new Person() { Family = new List<Person>() { person } };
            person.Family.Add(newPerson);
            var identation = new string('\t', 8);
            var printer = ObjectPrinter.For<Person>();

            var result = printer.PrintToString(person);

            result.Should()
                .Contain(
                    $"{nameof(person.Family)} = {Environment.NewLine}{identation}This object has already been printed");
        }
    }
}