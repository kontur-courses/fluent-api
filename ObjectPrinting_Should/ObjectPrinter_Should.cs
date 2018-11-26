using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Tests;

namespace ObjectPrinting_Should
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        private PrintingConfig<Person> printer;
        private Person person;

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
            person = new Person
            {
                Name = "Romutchio",
                Age = 18,
                Height = 1.82,
            };
        }

        [Test]
        public void ObjectPrinter_ShouldExcludeTypeFromSerialization()
        {
            var expected = string.Join(Environment.NewLine,
                               "Person", "\tId = Guid", "\tName = Romutchio",
                               "\tHeight = 1,82") + Environment.NewLine;
            printer.Excluding<int>();
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldChangeSerializationForType()
        {
            var person = new Person { Name = "Elizabeth", Age = 42 };
            var expected = string.Join(Environment.NewLine,
                "Person",
                "\tId = Guid",
                "\tName = Elizabeth",
                "\tHeight = 0",
                "\tAge = DoNotAskGirlAboutHerAge") + Environment.NewLine;
            printer.Printing<int>().Using(s => "DoNotAskGirlAboutHerAge");
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldChangeCultureForDouble()
        {
            var expected = string.Join(Environment.NewLine,
                               "Person", "\tId = Guid", "\tName = Romutchio",
                               "\tHeight = 1.82",
                               "\tAge = 18") + Environment.NewLine;

            printer.Printing<double>().Using(CultureInfo.InvariantCulture);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldExcludePropertyFromSerialization()
        {
            var expected = string.Join(Environment.NewLine,
                               "Person", "\tId = Guid",
                               "\tHeight = 1,82",
                               "\tAge = 18") + Environment.NewLine;
            printer.Excluding(p => p.Name).PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldApplyTrimFunctionForStrings()
        {
            var expected = string.Join(Environment.NewLine,
                               "Person", "\tId = Guid", "\tName = Rom",
                               "\tHeight = 1,82",
                               "\tAge = 18") + Environment.NewLine;
            printer.Printing<string>().TrimmedToLength(3);
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldChangeSerializationForSelectedProperty()
        {
            var expected = string.Join(Environment.NewLine,
                               "Person", "\tId = Guid", "\tName = Romutchio",
                               "\tHeight = 1,82",
                               "\tAge = 12") + Environment.NewLine;
            printer.Printing(p => p.Age).Using(i => i.ToString("X"));
            printer.PrintToString(person).Should().Be(expected);
        }

        [Test]
        public void ObjectPrinter_ShouldBeImmutable()
        {
            var withoutAge = string.Join(Environment.NewLine,
                               "Person", "\tId = Guid", "\tName = Romutchio",
                               "\tHeight = 1,82") + Environment.NewLine;
            var withoutHeight = string.Join(Environment.NewLine,
                                 "Person", "\tId = Guid", "\tName = Romutchio",
                                 "\tAge = 18") + Environment.NewLine;
            printer.Excluding(p => p.Age).PrintToString(person).Should().Be(withoutAge);
            printer.Excluding(p => p.Height).PrintToString(person).Should().Be(withoutHeight);
            
        }

        [Test]
        public void ObjectPrinter_ShouldMaintainEnumberables()
        {
            var collection = new[] {8,9,5,0,5,5,7,9,8,9,6};
            var expected = string.Join(Environment.NewLine,
                               "Int32[]", "{", "\t8", "\t9", "\t5", "\t0", "\t5", 
                               "\t5", "\t7", "\t9", "\t8", "\t9", "\t6","}") + Environment.NewLine;
            var printer = ObjectPrinter.For<int[]>();
            printer.PrintToString(collection).Should().Be(expected);
        }


    }
}
