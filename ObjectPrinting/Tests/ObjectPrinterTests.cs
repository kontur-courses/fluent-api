using System;
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
        public void ObjectPrinter_ShouldCorrectlyExcludeType()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<int>();
            printer.PrintToString(examplePerson).Should().Be("Person" + Environment.NewLine + '\t' + "Id = Guid" +
                                                             Environment.NewLine + '\t' + "Name = John Doe" +
                                                             Environment.NewLine + '\t' + "Height = 173.5" +
                                                             Environment.NewLine);
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectlyExcludeProperty()
        {
            var printer = ObjectPrinter.For<Person>().Excluding(p => p.Age);
            printer.PrintToString(examplePerson).Should().Be("Person" + Environment.NewLine + '\t' + "Id = Guid" +
                                                             Environment.NewLine + '\t' + "Name = John Doe" +
                                                             Environment.NewLine + '\t' + "Height = 173.5" +
                                                             Environment.NewLine);
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectlySetCultureForNumericalTypes()
        {
            var printer = ObjectPrinter.For<Person>().Printing<double>().Using(new CultureInfo("ru"));
            printer.PrintToString(examplePerson).Should().Be("Person" + Environment.NewLine + '\t' + "Id = Guid" +
                                                             Environment.NewLine + '\t' + "Name = John Doe" +
                                                             Environment.NewLine + '\t' + "Height = 173,5" +
                                                             Environment.NewLine + '\t' + "Age = 42" +
                                                             Environment.NewLine);
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectlyApplySpecialSerializationForType()
        {
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(i => i.ToString("X"));
            printer.PrintToString(examplePerson).Should().Be("Person" + Environment.NewLine + '\t' + "Id = Guid" +
                                                             Environment.NewLine + '\t' + "Name = John Doe" +
                                                             Environment.NewLine + '\t' + "Height = 173.5" +
                                                             Environment.NewLine + '\t' + "Age = 2A");
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectlyApplySpecialSerializationForProperties()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Age).Using(i => i.ToString("X"));
            printer.PrintToString(examplePerson).Should().Be("Person" + Environment.NewLine + '\t' + "Id = Guid" +
                                                             Environment.NewLine + '\t' + "Name = John Doe" +
                                                             Environment.NewLine + '\t' + "Height = 173.5" +
                                                             Environment.NewLine + '\t' + "Age = 2A");
        }

        [Test]
        public void ObjectPrinter_ShouldCorrectlyTrimStrings()
        {
            var printer = ObjectPrinter.For<Person>().Printing(p => p.Name).TrimmedToLength(5);
            printer.PrintToString(examplePerson).Should().Be("Person" + Environment.NewLine + '\t' + "Id = Guid" +
                                                             Environment.NewLine + '\t' + "Name = John " + '\t' +
                                                             "Height = 173.5" + Environment.NewLine + '\t' +
                                                             "Age = 42" + Environment.NewLine);
        }
    }
}