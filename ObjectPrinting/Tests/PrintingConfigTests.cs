using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class PrintingConfigTests
    {
        private Person testPerson;

        [SetUp]
        public void SetUp()
        {
            testPerson = new Person("John", 180, 32);
        }

        [Test]
        public void Exclude_ShouldExcludeTypes()
        {
            var resString = testPerson.PrintToString(config => config.Excluding<int>());
            resString.Should().Be("Person\r\n\tId = Guid\r\n\tName = John\r\n\tHeight = 180\r\n\tPet = null\r\n");
        }

        [Test]
        public void Exclude_ShouldExcludeProperties()
        {
            var resString = testPerson.PrintToString(config => config.Excluding(p => p.Name));
            resString.Should().Be("Person\r\n\tId = Guid\r\n\tHeight = 180\r\n\tAge = 32\r\n\tPet = null\r\n");
        }

        [Test]
        public void Exclude_ShouldNotExcludeThisPropertyInNestedTypes()
        {
            testPerson.Pet = new Pet("Lassie");
            var resString = testPerson.PrintToString(config => config.Excluding(p => p.Name));
            resString.Should()
                .Be("Person\r\n\tId = Guid\r\n\tHeight = 180\r\n\tAge = 32\r\n\tPet = Pet\r\n\t\tName = Lassie\r\n\t\tAge = 0");

        }

        //"Person\r\n\tId = Guid\r\n\tName = John\r\n\tHeight = 180\r\n\tAge = 32\r\n\tPet = Pet\r\n\t\tName = Lassie\r\n\t\tAge = 8\r\n"

        [Test]
        public void Exclude_ShouldNotExcludeThisTypeInNestedTypes()
        {
            testPerson.Pet = new Pet("Lassie") { Age = 8 };
            var resString = testPerson.PrintToString(config => config.Excluding<int>());
            resString.Should()
                .Be("Person\r\n\tId = Guid\r\n\tName = John\r\n\tHeight = 180\r\n\tPet = Pet\r\n\t\tName = Lassie\r\n\t\tAge = 8");
        }

        [Test]
        public void Printing_ShouldAllowToTunePrintMethodForType()
        {
            var resString = testPerson.PrintToString(config => config.Printing<string>().Using(s => s.ToUpper()));
            resString.Should().Be("Person\r\n\tId = Guid\r\n\tName = JOHN\r\n\tHeight = 180\r\n\tAge = 32\r\n\tPet = null\r\n");
        }

        [Test]
        public void Printing_ShouldAllowToTunePrintMethodForProperty()
        {
            var resString = testPerson.PrintToString(config => config.Printing(p => p.Age).Using(age => "0"));
            resString.Should().Be("Person\r\n\tId = Guid\r\n\tName = John\r\n\tHeight = 180\r\n\tAge = 0\r\n\tPet = null\r\n");
        }

        [Test]
        public void Printing_ShouldAllowToSetCultureInfoForNumbersByPropertyName()
        {
            testPerson.Height = 180.7;
            var resString = testPerson.PrintToString(config => config.Printing(p => p.Height).ChangeCultureInfo(CultureInfo.CreateSpecificCulture("zu-Z")));
            resString.Should().Be("Person\r\n\tId = Guid\r\n\tName = John\r\n\tHeight = 180.7\r\n\tAge = 32\r\n\tPet = null\r\n");
        }

        [Test]
        public void Printing_ShouldAllowToSetCultureInfoForNumbersByType()
        {
            testPerson.Height = 180.7;
            var resString = testPerson.PrintToString(config => config.Printing<double>().ChangeCultureInfo(CultureInfo.CreateSpecificCulture("zu-Z")));
            resString.Should().Be("Person\r\n\tId = Guid\r\n\tName = John\r\n\tHeight = 180.7\r\n\tAge = 32\r\n\tPet = null\r\n");
        }

        [Test]
        public void Printing_ShouldAllowToSetLengthInfoForStringsByPropertyName()
        {
            var resString = testPerson.PrintToString(config => config.Printing(p => p.Name).TrimmedToLength(1));
            resString.Should().Be("Person\r\n\tId = Guid\r\n\tName = J\r\n\tHeight = 180\r\n\tAge = 32\r\n\tPet = null\r\n");
        }

        [Test]
        public void Printing_ShouldAllowToSetLengthInfoForStringsByType()
        {
            var resString = testPerson.PrintToString(config => config.Printing<string>().TrimmedToLength(1));
            resString.Should().Be("Person\r\n\tId = Guid\r\n\tName = J\r\n\tHeight = 180\r\n\tAge = 32\r\n\tPet = null\r\n");
        }

        [Test]
        public void XXX()
        {
            testPerson.Surname = "Simpson";
            var res = testPerson.PrintToString(c =>
                c.Printing<string>().TrimmedToLength(1)
                    .Printing(p => p.Surname).Using(s => s.ToUpper())
                    );
        }







    }
}