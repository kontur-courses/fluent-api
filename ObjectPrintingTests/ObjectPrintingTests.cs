using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrintingTests
    {
        private Person person;
        private string printedPerson;

        [OneTimeSetUp]
        public void Init()
        {
            person = new Person
            {
                Name = "Alexander",
                Age = 19,
                Height = 5.5,
            };
        }

        [Test]
        public void Should_Exclude_SpecifiedType()
        {
            var propertyName = nameof(Person.Name);
            person.PrintToString().Should().Contain(propertyName);
            person.PrintToString(c => c.Excluding<string>()).Should().NotContain(propertyName);
        }

        [Test]
        public void Should_Exclude_SpecifiedMember()
        {
            var propertyName = nameof(person.UselessField);
            person.PrintToString().Should().Contain(propertyName);
            person.PrintToString(c => c.Excluding(p => p.UselessField)).Should().NotContain(propertyName);
        }

        [Test]
        public void Should_UseSpecifiedSerialization_ForSpecifiedType()
        {
            person.PrintToString().Should().Contain("Age = 19");
            person.PrintToString(c => c.Printing<int>().Using(i => i.ToString("X"))).Should().Contain("Age = 13");
        }

        [Test]
        public void Should_UseSpecifiedSerialization_ForSpecifiedMember()
        {
            person.PrintToString().Should().Contain("Age = 19");
            person.PrintToString(c => c.Printing(p => p.Age).Using(i => i.ToString("X"))).Should().Contain("Age = 13");
        }

        [Test]
        public void Should_UseSpecifiedCulture_ForSpecifiedType()
        {
            person.PrintToString().Should().Contain("Height = 5,5");
            person.PrintToString(c => c.Printing<double>().Using(CultureInfo.InvariantCulture)).Should().Contain("Height = 5.5");
        }

        [Test]
        public void Should_TrimSelectedStringMember_ToFiveChars()
        {
            person.PrintToString(c => c.Printing(p => p.Name).TrimmedToLength(5)).Should().Contain("Name = Alexa");
        }

        
    }
}