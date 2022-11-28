using NUnit.Framework;
using FluentAssertions;
using System;
using ObjectPrinting.PropertyPrintingConfig;

namespace ObjectPrinting.Tests
{
    class PropertyPrintingConfigTests
    {
        private readonly Person person = new Person { Name = "Alex", Age = 19 };
        private PrintingConfig<Person> printingConfig;

        [SetUp]
        public void CreateObjectPrinter()
        {
            printingConfig = ObjectPrinter.For<Person>();
        }

        [Test]
        public void PrintingUsing_PropertyByName_Success()
        {
            var printedObject = printingConfig.Printing(p => p.Age).Using(val => $"{val} years").PrintToString(person);

            printedObject.Should().Contain($"{person.Age} years");

            printingConfig.PrintToString(person).Should().NotContain("years");
        }

        [Test]
        public void PrintingUsing_PropertyByNameTwice_ThrowException()
        {
            Action act = () => printingConfig.Printing(p => p.Age).Using(val => $"{val} years")
                .Printing(p => p.Age).Using(val => val.ToString("X8"));

            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void PrintingUsing_PropertyByType_Success()
        {
            var printedObject = printingConfig.Printing<Guid>().Using(val => $"{val} (GUID)").PrintToString(person);

            printedObject.Should().Contain($"{person.Id} (GUID)");

            printingConfig.PrintToString(person).Should().NotContain("(GUID)");
        }

        [Test]
        public void PrintingUsing_PropertyByTypeTwice_ThrowException()
        {
            Action act = () => printingConfig.Printing<Guid>().Using(val => $"{val} (GUID)")
                .Printing<Guid>().Using(val => val.ToString());

            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void PrintingUsing_PropertyTrim_Success()
        {
            var printedObject = printingConfig.Printing(p=>p.Name).TrimmedToLength(3).PrintToString(person);

            printedObject.Should().Contain($"{nameof(Person.Name)} = {person.Name.Substring(0,3)}");

            printingConfig.PrintToString(person).Should().NotContain("(GUID)");
        }
    }
}
