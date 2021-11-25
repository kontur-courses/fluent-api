using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests
{
    public class ObjectPrintingTests
    {
        private Person person;
        private string newLine;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            person = new Person { Name = "Alex", Age = 19, Height = 170.1, Id = Guid.NewGuid()};
            newLine = Environment.NewLine;
        }

        [Test]
        public void AcceptanceTest()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<int>().Using(i => i.ToString("X"))
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .Printing(x => x.Height).Using(i => i.ToString("P"))
                .Printing(p => p.Name).TrimmedToLength(10)
                .Excluding(p => p.Age);

            var s1 = printer.PrintToString(person);
            var s2 = person.PrintToString();
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age).Excluding(p => p.Name));

            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        [Test]
        public void ShouldExcludeMember_WhenItsTypeExcluded()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>();

            var actual = printer.PrintToString(person);

            actual.Should().Be($"Person{newLine}\tId = Guid{newLine}\tName = Alex{newLine}\tHeight = 170,1{newLine}");
        }

        [Test]
        public void ShouldExcludeMember_WhenItsPathExcluded()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Name);

            var actual = printer.PrintToString(person);

            actual.Should().Be($"Person{newLine}\tId = Guid{newLine}\tHeight = 170,1{newLine}\tAge = 19{newLine}");
        }

        [Test]
        public void ShouldUseCustomTypeSerializer_WhenSpecifiedSerializerForType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>()
                .Using(x => x.ToString("X"));

            var actual = printer.PrintToString(person);

            actual.Should().Be($"Person{newLine}\tId = Guid{newLine}\tName = Alex{newLine}\tHeight = 170,1{newLine}\tAge = 13{newLine}");
        }

        [Test]
        public void ShouldUseCustomMemberSerializer_WhenSpecifiedSerializerForMember()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Age)
                .Using(x => x.ToString("X"));

            var actual = printer.PrintToString(person);

            actual.Should().Be($"Person{newLine}\tId = Guid{newLine}\tName = Alex{newLine}\tHeight = 170,1{newLine}\tAge = 13{newLine}");
        }

        [Test]
        public void ShouldUseCustomTypeCulture_WhenSpecifiedCultureForType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(CultureInfo.GetCultureInfo("en-US"));

            var actual = printer.PrintToString(person);

            actual.Should().Be($"Person{newLine}\tId = Guid{newLine}\tName = Alex{newLine}\tHeight = 170.1{newLine}\tAge = 19{newLine}");
        }

        [Test]
        public void ShouldUseCustomMemberCulture_WhenSpecifiedCultureForMember()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Height)
                .Using(CultureInfo.GetCultureInfo("en-US"));

            var actual = printer.PrintToString(person);

            actual.Should().Be($"Person{newLine}\tId = Guid{newLine}\tName = Alex{newLine}\tHeight = 170.1{newLine}\tAge = 19{newLine}");
        }

        [Test]
        public void ShouldUseTrimming_WhenItsSpecifiedForType()
        {
            var person = new Person { Name = "Alexalexalex", Age = 19, Height = 170.1, Id = Guid.NewGuid() };
            var printer = ObjectPrinter.For<Person>()
                .Printing<string>()
                .TrimmedToLength(10);

            var actual = printer.PrintToString(person);

            actual.Should().Be($"Person{newLine}\tId = Guid{newLine}\tName = Alexalexal{newLine}\tHeight = 170,1{newLine}\tAge = 19{newLine}");
        }

        [Test]
        public void ShouldUseTrimming_WhenItsSpecifiedForMember()
        {
            var person = new Person { Name = "Alexalexalex", Age = 19, Height = 170.1, Id = Guid.NewGuid() };
            var printer = ObjectPrinter.For<Person>()
                .Printing(x => x.Name)
                .TrimmedToLength(10);

            var actual = printer.PrintToString(person);

            actual.Should().Be($"Person{newLine}\tId = Guid{newLine}\tName = Alexalexal{newLine}\tHeight = 170,1{newLine}\tAge = 19{newLine}");
        }
    }
}