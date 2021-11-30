using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrintingTask;
using ObjectPrintingTask.Extensions;
using ObjectPrintingTask.PrintingConfiguration;
using ObjectPrintingTests.TestData;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
        }

        private readonly Person person = Person.GetTestInstance();
        private PrintingConfig<Person> printer;

        [Test]
        public void AcceptanceTest_ExcludeMembersAndTypes()
        {
            printer
                .Excluding<Guid>()
                .Excluding(p => p.Surname);

            Console.WriteLine(printer.PrintToString(person));
        }

        [Test]
        public void AcceptanceTest_SpecifyAlternateScenarios()
        {
            printer
                .PrintingType<Guid>().Using(g => g.ToString().Substring(0, 8))
                .PrintingMember(p => p.Height).Using(height => (height / 100).ToString("#.##m"));

            Console.WriteLine(printer.PrintToString(person));
        }

        [Test]
        public void AcceptanceTest_SpecifyCultureInfo()
        {
            printer
                .PrintingType<double>().Using(CultureInfo.GetCultureInfo("RU-ru"));

            Console.WriteLine(printer.PrintToString(person));
        }

        [Test]
        public void AcceptanceTest_SpecifyStringMembersLength()
        {
            printer
                .PrintingMember(p => p.Name).TrimmedToLength(3);

            Console.WriteLine(printer.PrintToString(person));
        }

        [Test]
        public void AcceptanceTest_PrintingCollections()
        {
            Console.WriteLine(printer.PrintToString(person));
        }
    }
}