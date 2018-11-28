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
                Weight = 58,
                LuckyNumbers = new int[] { 3, 1, 45, 6, 0, -2, 23, 66, 1, 10, 11, 123 },
                Parent = new Person { Child = person }
            };

            printedPerson = "Person\r\n\tId = 00000000-0000-0000-0000-000000000000\r\n\tName = Alexander\r\n\tHeight = 5,5\r\n\tAge = 19\r\n\tParent = Person\r\n\t\tId = 00000000-0000-0000-0000-000000000000\r\n\t\tName = null\r\n\t\tHeight = 0\r\n\t\tAge = 0\r\n\t\tParent = null\r\n\t\tChild = null\r\n\t\tLuckyNumbers = null\r\n\t\tWeight = 0\r\n\t\tUselessField = 0\r\n\tChild = null\r\n\tLuckyNumbers = Int32[]\r\n\t\t3\r\n\t\t1\r\n\t\t45\r\n\t\t...\r\n\tWeight = 58\r\n\tUselessField = 0\r\n";
        }

        [Test]
        public void Should_Print_Person()
        {
            
            person.PrintToString().Should().Be(printedPerson);
        }

        [Test]
        public void Should_Exclude_SpecifiedType()
        {
            var propertyName = nameof(Person.Name);
            person.PrintToString().Should().Contain(propertyName);
            person.PrintToString(c => c.Excluding<string>()).Should().NotContain(propertyName);
        }

        [Test]
        public void Should_Exclude_SpecifiedProperty()
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
        public void Should_UseSpecifiedSerialization_ForSpecifiedProperty()
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
        public void Should_TrimSelectedStringProperty_ToFiveChars()
        {
            person.PrintToString(c => c.Printing(p => p.Name).TrimmedToLength(5)).Should().Contain("Name = Alexa");
        }
    }
}