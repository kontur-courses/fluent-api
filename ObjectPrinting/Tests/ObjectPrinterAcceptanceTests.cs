using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            _person = new Person
            {
                Name = "Alex", 
                Age = 19, 
                Height = 200.02, 
                Country = "Russia", 
                Weight = 100,
                Friend = new Person()
                {
                    Name = "Anton", 
                    Age = 20, 
                    Height = 160, 
                    Country = "Russia", 
                    Weight = 80
                }
            };
        }

        [Test]
        public void PrintToString_PrintArray()
        {
            var persons = new[] { _person, _person };
            
            var friendOfMyFriend = _person.Friend.Friend == null ? "null" : _person.Friend.Friend.GetType().ToString();
            var expectedResult = 
                $"Person\r\n\tId = {_person.Id}\r\n\tName = {_person.Name}\r\n\tHeight = {_person.Height}\r\n\tAge = {_person.Age}\r\n\tFriend = Person\r\n\t\tId = {_person.Friend.Id}\r\n\t\tName = {_person.Friend.Name}\r\n\t\tHeight = {_person.Friend.Height}\r\n\t\tAge = {_person.Friend.Age}\r\n\t\tFriend = {friendOfMyFriend}\r\n\t\tWeight = {_person.Friend.Weight}\r\n\t\tCountry = {_person.Friend.Country}\r\n\tWeight = {_person.Weight}\r\n\tCountry = {_person.Country}\r\nPerson\r\n\tId = {_person.Id}\r\n\tName = {_person.Name}\r\n\tHeight = {_person.Height}\r\n\tAge = {_person.Age}\r\n\tFriend = Person\r\n\t\tId = {_person.Friend.Id}\r\n\t\tName = {_person.Friend.Name}\r\n\t\tHeight = {_person.Friend.Height}\r\n\t\tAge = {_person.Friend.Age}\r\n\t\tFriend = {friendOfMyFriend}\r\n\t\tWeight = {_person.Friend.Weight}\r\n\t\tCountry = {_person.Friend.Country}\r\n\tWeight = {_person.Weight}\r\n\tCountry = {_person.Country}\r\n";
            
            ObjectPrinter
                .For<Person>()
                .PrintToString(persons)
                .Should()
                .Be(expectedResult);
        }
        
        [Test]
        public void PrintToString_PrintDictionary()
        {
            var persons = new Dictionary<string, Person>()
            {
                { "1", _person },
                { "2", _person },
            };

            var friendOfMyFriend = _person.Friend.Friend == null ? "null" : _person.Friend.Friend.GetType().ToString();
            
            var expectedResult = 
                $"Person\r\n\tId = {_person.Id}\r\n\tName = {_person.Name}\r\n\tHeight = {_person.Height}\r\n\tAge = {_person.Age}\r\n\tFriend = Person\r\n\t\tId = {_person.Friend.Id}\r\n\t\tName = {_person.Friend.Name}\r\n\t\tHeight = {_person.Friend.Height}\r\n\t\tAge = {_person.Friend.Age}\r\n\t\tFriend = {friendOfMyFriend}\r\n\t\tWeight = {_person.Friend.Weight}\r\n\t\tCountry = {_person.Friend.Country}\r\n\tWeight = {_person.Weight}\r\n\tCountry = {_person.Country}\r\nPerson\r\n\tId = {_person.Id}\r\n\tName = {_person.Name}\r\n\tHeight = {_person.Height}\r\n\tAge = {_person.Age}\r\n\tFriend = Person\r\n\t\tId = {_person.Friend.Id}\r\n\t\tName = {_person.Friend.Name}\r\n\t\tHeight = {_person.Friend.Height}\r\n\t\tAge = {_person.Friend.Age}\r\n\t\tFriend = {friendOfMyFriend}\r\n\t\tWeight = {_person.Friend.Weight}\r\n\t\tCountry = {_person.Friend.Country}\r\n\tWeight = {_person.Weight}\r\n\tCountry = {_person.Country}\r\n";
            
            ObjectPrinter
                .For<Person>()
                .PrintToString(persons)
                .Should()
                .Be(expectedResult);
        }
        
        [Test]
        public void PrintToString_PrintList()
        {
            var persons = new List<Person>()
            {
                _person,
                _person
            };
            
            var friendOfMyFriend = _person.Friend.Friend == null ? "null" : _person.Friend.Friend.GetType().ToString();
            var expectedResult = 
                $"Person\r\n\tId = {_person.Id}\r\n\tName = {_person.Name}\r\n\tHeight = {_person.Height}\r\n\tAge = {_person.Age}\r\n\tFriend = Person\r\n\t\tId = {_person.Friend.Id}\r\n\t\tName = {_person.Friend.Name}\r\n\t\tHeight = {_person.Friend.Height}\r\n\t\tAge = {_person.Friend.Age}\r\n\t\tFriend = {friendOfMyFriend}\r\n\t\tWeight = {_person.Friend.Weight}\r\n\t\tCountry = {_person.Friend.Country}\r\n\tWeight = {_person.Weight}\r\n\tCountry = {_person.Country}\r\nPerson\r\n\tId = {_person.Id}\r\n\tName = {_person.Name}\r\n\tHeight = {_person.Height}\r\n\tAge = {_person.Age}\r\n\tFriend = Person\r\n\t\tId = {_person.Friend.Id}\r\n\t\tName = {_person.Friend.Name}\r\n\t\tHeight = {_person.Friend.Height}\r\n\t\tAge = {_person.Friend.Age}\r\n\t\tFriend = {friendOfMyFriend}\r\n\t\tWeight = {_person.Friend.Weight}\r\n\t\tCountry = {_person.Friend.Country}\r\n\tWeight = {_person.Weight}\r\n\tCountry = {_person.Country}\r\n";
            
            ObjectPrinter
                .For<Person>()
                .PrintToString(persons)
                .Should()
                .Be(expectedResult);
            
        }
        
        [Test]
        public void PrintToString_PrintsDefaultString_WhenNoOptions()
        {
            var actualString = ObjectPrinter
                .For<Person>()
                .PrintToString(_person);

            var friendOfMyFriend = _person.Friend.Friend == null ? "null" : _person.Friend.Friend.GetType().ToString();
            var expectedSting =
                $"Person\r\n\tId = {_person.Id}\r\n\tName = {_person.Name}\r\n\tHeight = {_person.Height}\r\n\tAge = {_person.Age}\r\n\tFriend = Person\r\n\t\tId = {_person.Friend.Id}\r\n\t\tName = {_person.Friend.Name}\r\n\t\tHeight = {_person.Friend.Height}\r\n\t\tAge = {_person.Friend.Age}\r\n\t\tFriend = {friendOfMyFriend}\r\n\t\tWeight = {_person.Friend.Weight}\r\n\t\tCountry = {_person.Friend.Country}\r\n\tWeight = {_person.Weight}\r\n\tCountry = {_person.Country}\r\n";

            expectedSting.Should().Be(actualString);
        }

        [Test]
        public void PrintToString_DontPrintPropertyWithExcludedType()
        {
            var actualString = ObjectPrinter.For<Person>()
               .Exclude<Guid>()
               .PrintToString(_person);

            actualString.Should().NotContain(_person.Id.ToString());
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
            var maxStringLength = 10;
            ObjectPrinter.For<Person>()
                .MaxLength(maxStringLength)
                .PrintToString(_person)
                .Split(Environment.NewLine)
                .ToList()
                .Any(s => s.Trim().Length >= 10)
                .Should()
                .BeFalse();
        }
    }
}