using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person _person;
        
        [SetUp]
        public void CreatePerson()
        {
            _person = new Person { Name = "Alex", Age = 19, Height = 200.02, Id = "1", Country = "Russia", Weight = 100 };
        }

        [Test]
        public void PrintToString_PrintArray()
        {
            var persons = new[] { _person, _person };
            
            ObjectPrinter
                .For<Person>()
                .PrintToString(persons)
                .Should()
                .Be("Person\r\n\tId = 1\r\n\tName = Alex\r\n\tHeight = 200,02\r\n\tAge = 19\r\n\tWeight = 100\r\n\tCountry = Russia\r\nPerson\r\n\tId = 1\r\n\tName = Alex\r\n\tHeight = 200,02\r\n\tAge = 19\r\n\tWeight = 100\r\n\tCountry = Russia\r\n");
        }
        
        [Test]
        public void PrintToString_PrintDictionary()
        {
            var persons = new Dictionary<string, Person>()
            {
                { "1", _person },
                { "2", _person },
            };
            
            ObjectPrinter
                .For<Person>()
                .PrintToString(persons)
                .Should()
                .Be("Person\r\n\tId = 1\r\n\tName = Alex\r\n\tHeight = 200,02\r\n\tAge = 19\r\n\tWeight = 100\r\n\tCountry = Russia\r\nPerson\r\n\tId = 1\r\n\tName = Alex\r\n\tHeight = 200,02\r\n\tAge = 19\r\n\tWeight = 100\r\n\tCountry = Russia\r\n");
        }
        
        [Test]
        public void PrintToString_PrintList()
        {
            var persons = new List<Person>()
            {
                _person,
                _person
            };
            
            ObjectPrinter
                .For<Person>()
                .PrintToString(persons)
                .Should()
                .Be("Person\r\n\tId = 1\r\n\tName = Alex\r\n\tHeight = 200,02\r\n\tAge = 19\r\n\tWeight = 100\r\n\tCountry = Russia\r\nPerson\r\n\tId = 1\r\n\tName = Alex\r\n\tHeight = 200,02\r\n\tAge = 19\r\n\tWeight = 100\r\n\tCountry = Russia\r\n");
        }
        
        [Test]
        public void PrintToString_PrintsDefaultString_WhenNoOptions()
        {
            var actualString = ObjectPrinter
                .For<Person>()
                .PrintToString(_person);

            var expectedSting = $"Person\r\n\tId = 1\r\n\tName = Alex\r\n\tHeight = 200,02\r\n\tAge = 19\r\n\tWeight = 100\r\n\tCountry = Russia\r\n";

            expectedSting.Should().Be(actualString);
        }

        [Test]
        public void PrintToString_DontPrintPropertyWithExcludedType()
        {
            var actualString = ObjectPrinter.For<Person>()
               .Exclude<Guid>()
               .PrintToString(_person);

            actualString.Should().NotContain("Guid");
        }

        [Test]
        public void PrintToString_DontPrintExcludedProperty()
        {
            var actualString = ObjectPrinter.For<Person>()
               .Exclude(p => p.Height)
               .PrintToString(_person);

            actualString.Should().NotContain("Height");
        }

        [Test]
        public void PrintToString_PrintsPropertiesWithTypeSerializationOption()
        {
            var actualString = ObjectPrinter.For<Person>()
                .CustomSerialize<string>()
                .Using(i => i.GetValue(_person) + " :)")
                .PrintToString(_person);

            actualString.Should().Contain(_person.Name + " :)");
        }

        [Test]
        public void PrintToString_PrintsNumericalTypeWithSpecifiedCulture()
        {
            var culture = new CultureInfo("en");

            var actualString = ObjectPrinter.For<Person>()
                .CustomSerialize<double>()
                .Using(propertyInfo => string.Format(propertyInfo.GetValue(_person).ToString(), culture))
                .PrintToString(_person);

            actualString.Should().Contain($"\tWeight = {_person.Weight.ToString(null, culture)}\r\n");
        }

        [Test]
        public void PrintToString_PrintsPropertyWithSerializationOption()
        {
            var actualString = ObjectPrinter.For<Person>()
                .CustomSerialize(p => p.Age)
                .Using(age => $"{age.GetValue(_person)} years old")
                .PrintToString(_person);

            actualString.Should().Contain($"\tAge = {_person.Age} years old\r\n");
        }
        
        [Test]
        public void PrintToString_PrintsWithMaxLength()
        {
            var actualString = ObjectPrinter.For<Person>()
                .MaxLength(10)
                .PrintToString(_person);

            var expectedSting = $"Person\r\n\tId = 1\r\n\tName = A\r\n\tHeight =\r\n\tAge = 19\r\n\tWeight =\r\n\tCountry \r\n";
            
            actualString.Should().Be(expectedSting);
        }
    }
}