using System;
using System.ComponentModel;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterTests
    {
        private static string defaultPersonSerialization;
        private static Person person;
        private static PrintingConfig<Person> personPrinter;
        private static string newLine;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            person = new Person
            {
                Name = "Alex",
                Age = 21,
                Height = 170.5,
            };
            newLine = Environment.NewLine;
            defaultPersonSerialization = 
                $"Person{newLine}\tId = 00000000-0000-0000-0000-000000000000{newLine}\tName = Alex{newLine}\tAge = 21{newLine}\tHeight = 170,5{newLine}";
        }

        [SetUp]
        public void SetUp()
        {
            personPrinter = ObjectPrinter.For<Person>();
        }


        [Test]
        public void AcceptanceTest()
        {
            var defaultResult = personPrinter.PrintToString(person);
            Console.WriteLine(defaultResult);
            var excludingResult = personPrinter
                .Excluding<Guid>()
                .PrintToString(person);
            Console.WriteLine(excludingResult);
            var specifiedTypeSerialization = personPrinter
                .Printing<int>().Using(i => i.ToString("F"))
                .PrintToString(person);
            Console.WriteLine(specifiedTypeSerialization);
            var customCultureResult = personPrinter
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .PrintToString(person);
            Console.WriteLine(customCultureResult);
            var specifiedPropertySerialization = personPrinter
                .Printing(p => p.Name).Using(p => p[0..3].ToString())
                .PrintToString(person);
            Console.WriteLine(specifiedPropertySerialization);
            var trimmedString = personPrinter
                .Printing(p => p.Name).TrimmedToLength(2)
                .PrintToString(person);
            Console.WriteLine(trimmedString);
            var excludingMember = personPrinter
                .Excluding(p => p.Age)
                .PrintToString(person);
            Console.WriteLine(excludingMember);
        }

        [Test]
        public void PrintPublicPropertiesAndFields()
        {
            personPrinter.PrintToString(person)
                .Should()
                .Be(defaultPersonSerialization);
        }

        [Test]
        public void ExcludePropertyWithType_WhenItExists()
        {
            var result = personPrinter
                .Excluding<Guid>()
                .PrintToString(person);
            result.Should()
                .Be($"Person{newLine}\tName = Alex{newLine}\tAge = 21{newLine}\tHeight = 170,5{newLine}");
        }

        [Test]
        public void ExcludeFieldWithType()
        {
            var result = personPrinter
                .Excluding<double>()
                .PrintToString(person);
            result.Should()
                .Be(
                    $"Person{newLine}\tId = 00000000-0000-0000-0000-000000000000{newLine}\tName = Alex{newLine}\tAge = 21{newLine}");
        }

        [Test]
        public void ExcludeNothing_WhenObjectDoesNotContainsType()
        {
            var result = personPrinter
                .Excluding<float>()
                .PrintToString(person);
            result.Should()
                .Be(defaultPersonSerialization);
        }

        [Test]
        public void UseCustomSerialization_WhenSpecifiedForType()
        {
            var result = personPrinter
                .Printing<int>().Using(n => n.ToString("F"))
                .PrintToString(person);
            result.Should()
                .Be(
                    $"Person{newLine}\tId = 00000000-0000-0000-0000-000000000000{newLine}\tName = Alex{newLine}\tAge = 21,00{newLine}\tHeight = 170,5{newLine}");
        }

        [Test]
        public void UseCustomCulture_WhenSpecifiedForType()
        {
            var result = personPrinter
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .PrintToString(person);
            result.Should()
                .Be(
                    $"Person{newLine}\tId = 00000000-0000-0000-0000-000000000000{newLine}\tName = Alex{newLine}\tAge = 21{newLine}\tHeight = 170.5{newLine}");
        }

        [Test]
        public void UseLastSpecifiedCustomSerialization()
        {
            var result = personPrinter
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .Printing<double>().Using(n => n.ToString(null, new CultureInfo("ru-ru")))
                .PrintToString(person);
            result.Should()
                .Be(defaultPersonSerialization);
        }

        [Test]
        public void UseCustomSerializer_WhenSpecifiedForMember()
        {
            var result = personPrinter
                .Printing(p => p.Name).Using(n => n.ToUpper())
                .PrintToString(person);
            result.Should()
                .Be(
                    $"Person{newLine}\tId = 00000000-0000-0000-0000-000000000000{newLine}\tName = ALEX{newLine}\tAge = 21{newLine}\tHeight = 170,5{newLine}");
        }

        [Test]
        public void TrimString_WhenSpecifiedForStringMember()
        {
            var result = personPrinter
                .Printing(p => p.Name).TrimmedToLength(2)
                .PrintToString(person);
            result.Should()
                .Be($"Person{newLine}\tId = 00000000-0000-0000-0000-000000000000{newLine}\tName = Al{newLine}\tAge = 21{newLine}\tHeight = 170,5{newLine}");
        }

        [Test]
        public void Throw_WhenTrimLengthIsNegative()
        {
            Action act = () =>
            {
                personPrinter
                    .Printing(p => p.Name).TrimmedToLength(-1);
            };
            act.Should().Throw<ArgumentException>();
        }

        [Test]
        public void ExcludeSpecifiedMember()
        {
            var result = personPrinter
                .Excluding(p => p.Id)
                .PrintToString(person);
            result.Should()
                .Be($"Person{newLine}\tName = Alex{newLine}\tAge = 21{newLine}\tHeight = 170,5{newLine}");
        }

        [Test]
        public void PrintCycleReferenceMessage_WhenDefaultThrowOptionAndNoCustomSerializer()
        {
            var alex = new PersonWithFriend() { Name = "Alex", Age = 19 };
            var john = new PersonWithFriend() { Name = "John", Age = 19 };
            alex.Friend = john;
            john.Friend = alex;
            var result = personPrinter
                .Excluding<Guid>()
                .PrintToString(alex);
            result.Should()
                .Be("PersonWithFriend\r\n\tFriend = PersonWithFriend\r\n\t\tFriend = cycle link detected\r\n\t\tName = John\r\n\t\tAge = 19\r\n\t\tHeight = 0\r\n\tName = Alex\r\n\tAge = 19\r\n\tHeight = 0\r\n");
            Console.WriteLine(result);
        }

        [Test]
        public void UseDefaultSerializationForCycleMember_WhenSpecifiedForType()
        {
            var alex = new PersonWithFriend() { Name = "Alex", Age = 19 };
            var john = new PersonWithFriend() { Name = "John", Age = 19 };
            alex.Friend = john;
            john.Friend = alex;
            var result = personPrinter
                .PrintingCycleMember<PersonWithFriend>().Using(p => p.Name)
                .Excluding<Guid>()
                .PrintToString(alex);
            result.Should()
                .Be("PersonWithFriend\r\n\tFriend = PersonWithFriend\r\n\t\tFriend = Alex\r\n\t\tName = John\r\n\t\tAge = 19\r\n\t\tHeight = 0\r\n\tName = Alex\r\n\tAge = 19\r\n\tHeight = 0\r\n");
        }

        [Test]
        public void Throw_WhenCycleMemberDetectedAndThrowOption()
        {
            var alex = new PersonWithFriend() { Name = "Alex", Age = 19 };
            var john = new PersonWithFriend() { Name = "John", Age = 19 };
            alex.Friend = john;
            john.Friend = alex;
            Action act = () =>
            {
                personPrinter
                    .ThrowingIfCycleReference(true)
                    .PrintToString(alex);
            };
            act.Should().Throw<Exception>();
        }

        private class Person
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }

            public double Height;
        }

        private class PersonWithPet : Person
        {
            public Pet Pet { get; set; }
        }

        private class PersonWithFriend : Person
        {
            public PersonWithFriend Friend { get; set; }

        }

        private class Pet
        {
            public string Name { get; set; }
        }
    }
}