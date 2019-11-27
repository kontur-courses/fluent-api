using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    class ObjectPrinterTests
    {
        [Test]
        public void ObjectPrinter_PrintingWithoutChanges()
        {
            var printer = ObjectPrinter.For<Person>();

            var outString = printer.PrintToString(new Person());

            outString.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n	Age = 0\r\n");
        }

        [Test]
        public void ObjectPrinter_PrintingAsExtension()
        {
            var testingPerson = new Person();

            var outString = testingPerson.PrintToString();

            outString.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n	Age = 0\r\n");
        }

        [Test]
        public void ObjectPrinter_PrintingAsExtensionWithConfiguration()
        {
            var testingPerson = new Person();

            var outString = testingPerson.PrintToString(s => s.Excluding(p => p.Age));

            outString.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n");
        }

        [Test]
        public void ObjectPrinter_PrintingExceptType()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<int>();

            var outString = printer.PrintToString(new Person());

            outString.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n");
        }

        [Test]
        public void ObjectPrinter_PrintingWithCustomSerializer()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(a=>"int");

            var outString = printer.PrintToString(new Person());

            outString.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n	Age = int\r\n");
        }

        [TestCase(4.2, "de-DE", "Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 4,2\r\n	Age = 0\r\n")]
        [TestCase(4.2, "en-GB", "Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 4.2\r\n	Age = 0\r\n")]
        public void ObjectPrinter_PrintingDoubleWithCustomCulture(double height, string culture, string expected)
        {
            var testingPerson = new Person();
            testingPerson.Height = height;
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(new CultureInfo(culture));

            var outString = printer.PrintToString(testingPerson);

            outString.Should().Be(expected);
        }


        [Test]
        public void ObjectPrinter_PrintingWithExcludedAge()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(x => x.Age);

            var outString = printer.PrintToString(new Person());

            outString.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n");
        }

        [Test]
        public void ObjectPrinter_PrintingWithCustomSerializationForAge()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(x=>x.Age).Using(x=>"age");

            var outString = printer.PrintToString(new Person());

            outString.Should().Be("Person\r\n	Id = Guid\r\n	Name = null\r\n	Height = 0\r\n	Age = age\r\n");
        }

        [Test]
        public void ObjectPrinter_PrintingWithCustomStringCut()
        {
            var testingPerson = new Person();
            testingPerson.Name = "abcdefghi";
            var printer = ObjectPrinter.For<Person>()
                .Printing(x=>x.Name).TrimmedToLength(3);

            var outString = printer.PrintToString(testingPerson);

            outString.Should().Be("Person\r\n	Id = Guid\r\n	Name = abc\r\n	Height = 0\r\n	Age = 0\r\n");
        }
    }
}
