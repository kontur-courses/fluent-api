using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterTests
    {
        private Person examplePerson = new Person();

        [SetUp]
        public void SetUp()
        {
            examplePerson.Age = 42;
            examplePerson.Name = "John Doe";
            examplePerson.Id = new Guid();
            examplePerson.Height = 173.5;
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectlySerializeDictionary()
        {
            examplePerson.Pets = new Dictionary<int, string> {{1, "cat"}, {2, "dog"}, {3, "pig"}};
            var printer = ObjectPrinter.For<Person>().Excluding<List<List<int>>>().Excluding<int>();
            printer.PrintToString(examplePerson).Should().Be("Person" + Environment.NewLine + '\t' +
                                                             "Id = 00000000-0000-0000-0000-000000000000" +
                                                             Environment.NewLine + '\t' + "Name = John Doe" +
                                                             Environment.NewLine + '\t' + "Height = 173.5" +
                                                             Environment.NewLine + '\t' + "Pets : {" +
                                                             Environment.NewLine + '\t' + '\t' + '\t' + "1 : cat," +
                                                             Environment.NewLine + '\t' + '\t' + '\t' + "2 : dog," +
                                                             Environment.NewLine + '\t' + '\t' + '\t' + "3 : pig," +
                                                             Environment.NewLine + '\t' + '\t' + '}' +
                                                             Environment.NewLine);
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectlySerializeListOfLists()
        {
            examplePerson.FavoriteNumberSets = new List<List<int>>
                {new List<int> {1, 2, 3}, new List<int> {13, 99, 13}, new List<int>()};
            var printer = ObjectPrinter.For<Person>().Excluding<Dictionary<int, string>>().Excluding<int>();
            printer.PrintToString(examplePerson).Should().Be("Person" + Environment.NewLine + '\t' +
                                                             "Id = 00000000-0000-0000-0000-000000000000" +
                                                             Environment.NewLine + '\t' + "Name = John Doe" +
                                                             Environment.NewLine + '\t' + "Height = 173.5" +
                                                             Environment.NewLine + '\t' + "FavoriteNumberSets : [" +
                                                             Environment.NewLine + '\t' + '\t' + '\t' + "[" +
                                                             Environment.NewLine + '\t' + '\t' + '\t' + '\t' + "1," +
                                                             Environment.NewLine + '\t' + '\t' + '\t' + '\t' + "2," +
                                                             Environment.NewLine + '\t' + '\t' + '\t' + '\t' + "3," +
                                                             Environment.NewLine + '\t' + '\t' + '\t' + "]," +
                                                             Environment.NewLine + '\t' + '\t' + '\t' + "[" +
                                                             Environment.NewLine + '\t' + '\t' + '\t' + '\t' + "13," +
                                                             Environment.NewLine + '\t' + '\t' + '\t' + '\t' + "99," +
                                                             Environment.NewLine + '\t' + '\t' + '\t' + '\t' + "13," +
                                                             Environment.NewLine + '\t' + '\t' + '\t' + "]," +
                                                             Environment.NewLine + '\t' + '\t' + '\t' + "[" +
                                                             Environment.NewLine + '\t' + '\t' + '\t' + "]," +
                                                             Environment.NewLine + '\t' + '\t' + ']' +
                                                             Environment.NewLine);
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectlyExcludeType()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<List<List<int>>>().Excluding<int>();
            printer.PrintToString(examplePerson).Should().Be("Person" + Environment.NewLine + '\t' +
                                                             "Id = 00000000-0000-0000-0000-000000000000" +
                                                             Environment.NewLine + '\t' + "Name = John Doe" +
                                                             Environment.NewLine + '\t' + "Height = 173.5" +
                                                             Environment.NewLine + '\t' + "Pets = null" +
                                                             Environment.NewLine);
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectlyExcludeProperty()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<List<List<int>>>().Excluding(p => p.Age);
            printer.PrintToString(examplePerson).Should().Be("Person" + Environment.NewLine + '\t' +
                                                             "Id = 00000000-0000-0000-0000-000000000000" +
                                                             Environment.NewLine + '\t' + "Name = John Doe" +
                                                             Environment.NewLine + '\t' + "Height = 173.5" +
                                                             Environment.NewLine + '\t' + "Pets = null" +
                                                             Environment.NewLine);
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectlySetCultureForNumericalTypes()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<List<List<int>>>().Excluding<Dictionary<int, string>>()
                .Printing<double>()
                .Using(new CultureInfo("ru"));
            printer.PrintToString(examplePerson).Should().Be("Person" + Environment.NewLine + '\t' +
                                                             "Id = 00000000-0000-0000-0000-000000000000" +
                                                             Environment.NewLine + '\t' + "Name = John Doe" +
                                                             Environment.NewLine + '\t' + "Height = 173,5" +
                                                             Environment.NewLine + '\t' + "Age = 42" +
                                                             Environment.NewLine);
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectlyApplySpecialSerializationForType()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<List<List<int>>>().Printing<int>()
                .Using(i => i.ToString("X"));
            printer.PrintToString(examplePerson).Should().Be("Person" + Environment.NewLine + '\t' +
                                                             "Id = 00000000-0000-0000-0000-000000000000" +
                                                             Environment.NewLine + '\t' + "Name = John Doe" +
                                                             Environment.NewLine + '\t' + "Height = 173.5" +
                                                             Environment.NewLine + '\t' + "Age = 2A" +
                                                             Environment.NewLine + '\t' + "Pets = null" +
                                                             Environment.NewLine);
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectlyApplySpecialSerializationForProperties()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<List<List<int>>>().Excluding<Dictionary<int, string>>()
                .Printing(p => p.Age)
                .Using(i => i.ToString("X"));
            printer.PrintToString(examplePerson).Should().Be("Person" + Environment.NewLine + '\t' +
                                                             "Id = 00000000-0000-0000-0000-000000000000" +
                                                             Environment.NewLine + '\t' + "Name = John Doe" +
                                                             Environment.NewLine + '\t' + "Height = 173.5" +
                                                             Environment.NewLine + '\t' + "Age = 2A" +
                                                             Environment.NewLine);
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectlyTrimStrings()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<List<List<int>>>().Excluding<Dictionary<int, string>>()
                .Printing(p => p.Name)
                .TrimmedToLength(5);
            printer.PrintToString(examplePerson).Should().Be("Person" + Environment.NewLine + '\t' +
                                                             "Id = 00000000-0000-0000-0000-000000000000" +
                                                             Environment.NewLine + '\t' + "Name = John " +
                                                             Environment.NewLine + '\t' + "Height = 173.5" +
                                                             Environment.NewLine + '\t' + "Age = 42" +
                                                             Environment.NewLine);
        }
    }
}