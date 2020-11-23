using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class PrintingConfigTests
    {
        private PrintingConfig<Person> printer;

        private readonly Person me =
            new Person() {Age = 20, Height = 150.5, Id = Guid.Empty, Name = "Natasha"};

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
        }

        [Test]
        public void ExcludingType_PrintEverything_WhenExcludingMissingType()
        {
            printer.Excluding<DateTime>();
            printer.PrintToString(me).Should().Contain("Age = 20")
                .And.Contain("Height = 150,5")
                .And.Contain("Id =")
                .And.Contain("Name = Natasha");
        }

        [Test]
        public void ExcludingType_DoNotPrintStrings()
        {
            printer.Excluding<string>();
            printer.PrintToString(me).Should()
                .NotContain("Name = Natasha")
                .And.Contain("Age = 20")
                .And.Contain("Height = 150,5")
                .And.Contain("Id =");
        }

        [Test]
        public void ExcludingField()
        {
            printer.Excluding(person => person.Name);
            printer.PrintToString(me).Should()
                .NotContain("Name = Natasha")
                .And.Contain("Age = 20")
                .And.Contain("Height = 150,5")
                .And.Contain("Id =");
        }

        [Test]
        public void AddSerialization_ForType()
        {
            printer.Printing<string>().Using(str => '"' + str + '"');
            printer.PrintToString(me).Should()
                .Contain("Name = \"Natasha\"")
                .And.Contain("Age = 20")
                .And.Contain("Height = 150,5")
                .And.Contain("Id =");
        }

        [Test]
        public void AddCultureInfo_ForType()
        {
            printer.Printing<double>().Using(CultureInfo.InvariantCulture);
            printer.PrintToString(me).Should()
                .Contain("Height = 150.5")
                .And.Contain("Age = 20")
                .And.Contain("Id =")
                .And.Contain("Name = Natasha");
        }

        [Test]
        public void AddLengthTrim_ForProperty()
        {
            printer.Printing(p => p.Name).TrimmedToLength(3);
            printer.PrintToString(me).Should()
                .Contain("Name = Nat" + Environment.NewLine)
                .And.Contain("Age = 20")
                .And.Contain("Height = 150,5")
                .And.Contain("Id =");
        }

        [Test]
        public void AddSerialization_ForNumberProperty()
        {
            printer.Printing(p => p.Age).Using(age => age + ".00");
            printer.PrintToString(me).Should()
                .Contain("Age = 20.00")
                .And.Contain("Height = 150,5")
                .And.Contain("Id =")
                .And.Contain("Name = Natasha");
        }

        [Test]
        public void AddSerialization_ForStringProperty()
        {
            printer.Printing(p => p.Name).Using(name => name.ToUpper());
            printer.PrintToString(me).Should()
                .Contain("Name = NATASHA")
                .And.Contain("Age = 20")
                .And.Contain("Height = 150,5")
                .And.Contain("Id =");
        }
    }
}