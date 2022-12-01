using NUnit.Framework;
using FluentAssertions;
using System;
using System.Globalization;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    public class PropertyPrintingConfigTests
    {
        private readonly Person person = new Person { Name = "Alex", Age = 19 };
        private PrintingConfig<Person> printingConfig;

        [SetUp]
        public void CreateObjectPrinter()
        {
            printingConfig = ObjectPrinter.For<Person>();
        }

        [Test]
        public void Should_AssignRule_When_RuleIsAssignedOnceForSingeMember()
        {
            var printedObject = printingConfig.Printing(p => p.Age).Using(val => $"{val} years").PrintToString(person);

            printedObject.Should().Contain($"{person.Age} years");

            printingConfig.PrintToString(person).Should().NotContain("years");
        }

        [Test]
        public void Should_ThrowInvalidOperationException_When_RulesAreAssignedTwiceForSingeMember()
        {
            Action act = () => printingConfig.Printing(p => p.Age).Using(val => $"{val} years")
                .Printing(p => p.Age).Using(val => val.ToString("X8"));

            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void Should_AssignCultureInfo_When_CultureInfoIsAssignedOnceForSingeMember()
        {
            person.Height = 18.3M;

            var printedObject = printingConfig.Printing(p => p.Height).
                Using(CultureInfo.InvariantCulture).PrintToString(person);

            printedObject.Should().Contain("18.3");

            printingConfig.PrintToString(person).Should().NotContain("18.3");
        }

        [Test]
        public void Should_ThrowInvalidOperationException_When_CultureInfoAreAssignedTwiceForSingeMember()
        {
            Action act = () => printingConfig.Printing(p => p.Height).
                Using(CultureInfo.InvariantCulture).Printing(p => p.Height).
                Using(CultureInfo.InvariantCulture);

            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void Should_AssignRule_When_RuleIsAssignedOnceForSingeType()
        {
            var printedObject = printingConfig.Printing<Guid>().Using(val => $"{val} (GUID)").PrintToString(person);

            printedObject.Should().Contain($"{person.Id} (GUID)");

            printingConfig.PrintToString(person).Should().NotContain("(GUID)");
        }

        [Test]
        public void Should_ThrowInvalidOperationException_When_RulesAreAssignedTwiceForSingeType()
        {
            Action act = () => printingConfig.Printing<Guid>().Using(val => $"{val} (GUID)")
                .Printing<Guid>().Using(val => val.ToString());

            act.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void Should_TrimmedMember_When_MemberValueTrimmedToPositiveLength()
        {
            var printedObject = printingConfig.Printing(p=>p.Name).TrimmedToLength(1).PrintToString(person);

            printedObject.Should().Contain($"{nameof(Person.Name)} = {person.Name.Substring(0,1)}");

            printingConfig.PrintToString(person).Should().Contain(person.Name);
        }

        [Test]
        public void Should_ThrowArgumentException_When_MemberValueTrimmedToZeroLengthOrLess()
        {
            Action act = () => printingConfig.Printing(p => p.Name).TrimmedToLength(0);
            act.Should().Throw<ArgumentException>();

            act = () => printingConfig.Printing(p => p.Name).TrimmedToLength(-100);
            act.Should().Throw<ArgumentException>();
        }
    }
}
