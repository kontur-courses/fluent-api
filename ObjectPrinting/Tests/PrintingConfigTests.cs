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
            printer.PrintToString(me).Should().NotContain("Name = Natasha");
        }

        [Test]
        public void ExcludingField_Name()
        {
            printer.Excluding(person => person.Name);
            printer.PrintToString(me).Should().NotContain("Name = Natasha");
        }

        [Test]
        public void AddSerialization_ForStrings()
        {
            printer.Printing<string>().Using(str => '"' + str + '"');
            printer.PrintToString(me).Should().Contain("Name = \"Natasha\"");
        }

        [Test]
        public void AddCultureInfo_ForDouble()
        {
            printer.Printing<double>().Using(CultureInfo.InvariantCulture);
            printer.PrintToString(me).Should().Contain("Height = 150.5");
        }

        [Test]
        public void AddLengthTrim_ForName()
        {
            printer.Printing(p => p.Name).TrimmedToLength(3);
            printer.PrintToString(me).Should().Contain("Name = Nat\n");
        }
    }
}