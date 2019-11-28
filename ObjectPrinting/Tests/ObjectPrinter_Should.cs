using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        private Person testedInstance;

        [SetUp]
        public void SetUp()
        {
            var personWithCyclicReference = new Person();
            testedInstance = new Person
            {
                Age = 30,
                Height = 180.5,
                Id = Guid.Empty,
                Name = "Vasya",
                Parent = personWithCyclicReference,
                FavoriteMovies = new[] {"The Shawshank Redemption", "The Godfather", null},
                BestFriends = new List<Person> {new Person(), personWithCyclicReference, null}
            };
            personWithCyclicReference.Parent = testedInstance;
        }

        [Test]
        [Category("ExcludeCertainProperty")]
        public void ExcludeInt()
        {
            var serializationResult = testedInstance.PrintToString(s => s.Excluding<int>());
            serializationResult.Should().NotContain("Age =");
        }

        [Test]
        [Category("ExcludeCertainProperty")]
        public void ExcludeGuid()
        {
            var serializationResult = testedInstance.PrintToString(s => s.Excluding<Guid>());
            serializationResult.Should().NotContain("Id =");
        }

        [Test]
        [Category("ChangeCertainTypeSerializationFormat")]
        public void ChangeFinalTypeFormat()
        {
            var result = 1.PrintToString(n => n.Printing<int>().Using(i => i.ToString("C", new CultureInfo("en-us"))));

            result.Should().Contain("$1.00");
        }

        [Test]
        [Category("ChangeCertainTypeSerializationFormat")]
        public void ChangeComplexTypeFormat()
        {
            var result = testedInstance.PrintToString(p =>
                p.Printing<int>().Using(n => n.ToString("C", new CultureInfo("en-us"))));

            result.Should().Contain("Age = $0.00\r\n").And.Contain("Age = $30.00\r\n");
        }

        [Test]
        [Category("ChangeCertainTypeSerializationFormat")]
        public void ChangeCollectionTypeFormat()
        {
            var result = testedInstance.PrintToString(
                p => p.Printing<string[]>().Using(n => "Good way to serialize arrays!"));

            result.Should().Contain("FavoriteMovies = Good way to serialize arrays!");
        }

        [Test]
        [Category("ChangeCertainTypeSerializationFormat")]
        public void ChangeGenericTypeFormat()
        {
            var result = testedInstance.PrintToString(
                p => p.Printing<List<Person>>().Using(n => "Good way to serialize lists!"));

            result.Should().Contain("BestFriends = Good way to serialize lists!");
        }

        [Test]
        public void ChangeNumberTypeCulture()
        {
            var result = testedInstance.PrintToString(p =>
                p.Printing<double>().Using(new CultureInfo("ru")));

            result.Should().Contain("Height = 180,5\r\n").And.NotContain("Height = 180.5\r\n");
        }

        [Test]
        public void ChangeCertainPropertySerializationFormat()
        {
            var result = testedInstance.PrintToString(p =>
                p.Printing(person => person.Age).Using(age => age.ToString("C", new CultureInfo("en-us"))));

            result.Should().Contain("Age = $30.00\r\n").And.NotContain("Age = 30\r\n").And.Contain("Age = 0");
        }

        [Test]
        public void ExcludeCertainProperty()
        {
            var result = testedInstance.PrintToString(p => p.Excluding(person => person.Age));

            result.Should().NotContain("Age = 30\r\n").And.Contain("Age = 0\r\n");
        }

        [Test]
        public void TrimStringProperty()
        {
            var result = testedInstance.PrintToString(p => p.Printing(person => person.Name).TrimmedToLength(3));

            result.Should().Contain("Name = Vas").And.NotContain("Name = Vasya");
        }
    }
}