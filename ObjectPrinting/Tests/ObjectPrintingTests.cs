using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrintingTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person();
        }

        [Test]
        public void Excluding_Should_Exclude_Type()
        {
            person
                .PrintToString(config => config.Excluding<int>())
                .Should()
                .NotContainAll(new[] { "Age =", "phoneNumber =" });
        }

        [Test]
        public void Using_Should_Use_Specified_Serialization_Method_For_Type()
        {
            var serializedPerson = person
                 .PrintToString(config => config.Printing<int>().Using(i => "Здесь было целое число")
                                                 .Printing<double>().Using(i => "Здесь было дробное число"));

            serializedPerson.Should().ContainAll(new[] { "Здесь было дробное число", "Здесь было целое число" });
            serializedPerson.Should().NotContain("0");
        }

        [Test]
        public void Using_Should_Use_Given_Culture_For_Type()
        {
            person.Height = 12.13;

            var serializedPerson = person.PrintToString(config => config
                                                                    .Printing<double>()
                                                                    .Using(new CultureInfo("eu-ES")));
            serializedPerson.Should().Contain("12,13");
        }

        [Test]
        public void Using_Should_Specified_Serialization_Method_For_Specific_Property()
        {
            person.Name = "Ivan";
            person.Age = 12;

            var serializedPerson = person.PrintToString(config => config
                                                                    .Printing(p => p.Name).Using(name => "Alex")
                                                                    .Printing(p => p.Age).Using(age => "Возраст"));

            serializedPerson.Should().ContainAll(new[] { "Name = Alex", "Age = Возраст" });
            serializedPerson.Should().NotContainAll(new[] { "Age = 12", "Name = Ivan" });
        }

        [Test]
        public void Using_Should_Specified_Serialization_Method_For_Specific_Field()
        {
            person.address = "USA";
            person.phoneNumber = 123;

            var serializedPerson = person.PrintToString(config => config
                                                                    .Printing(p => p.address)
                                                                        .Using(address => "UK")
                                                                    .Printing(p => p.phoneNumber)
                                                                        .Using(phoneNumber => "000"));

            serializedPerson.Should().ContainAll(new[] { "address = UK", "phoneNumber = 000" });
            serializedPerson.Should().NotContainAll(new[] { "address = USA", "phoneNumber = 123" });
        }

        [Test]
        public void TrimmedToLength_Should_Trimmed_String()
        {
            person.Name = "Alex";
            person.address = "Russian Federation";

            var serializedPerson = person.PrintToString(config => config.Printing(p => p.Name).TrimmedToLength(2));

            serializedPerson.Should().Contain("Name = Al");
            serializedPerson.Should().NotContain("Name = Alex");
            serializedPerson.Should().Contain("address = Russian Federation");
        }

        [Test]
        public void Excluding_Should_Exclude_Specific_Field()
        {
            var serializedPerson = person.PrintToString(config => config.Excluding(p => p.address)
                                                                        .Excluding(p => p.phoneNumber));

            serializedPerson.Should().NotContain("address =");
            serializedPerson.Should().NotContain("phoneNumber =");
        }

        [Test]
        public void Circular_ReferencesShould_NotThrow_Exceptions()
        {
            var friend = new Person();
            person.Friend = friend;
            friend.Friend = person;

            var serializedPerson = person.PrintToString();
            var serializedFriend = friend.PrintToString();

            serializedPerson.Should().Contain("Friend = cycle");
            serializedFriend.Should().Contain("Friend = cycle");
        }
    }
}
