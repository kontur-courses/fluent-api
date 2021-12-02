using System;
using System.Collections.Generic;
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
                Height = 170.5
            };
            newLine = Environment.NewLine;
            defaultPersonSerialization =
                $"Person{newLine}" +
                $"\tId = 00000000-0000-0000-0000-000000000000{newLine}" +
                $"\tName = Alex{newLine}" +
                $"\tAge = 21{newLine}" +
                $"\tHeight = 170,5{newLine}";
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
                .Printing(p => p.Name).Using(p => p[..3].ToString())
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
            result
                .Should()
                .Be($"Person{newLine}" +
                    $"\tName = Alex{newLine}" +
                    $"\tAge = 21{newLine}" +
                    $"\tHeight = 170,5{newLine}");
        }

        [Test]
        public void AccumulateSettings()
        {
            var printer = personPrinter
                .Excluding<Guid>()
                .Excluding<int>();
            printer.PrintToString(person)
                .Should()
                .Be($"Person{newLine}" +
                    $"\tName = Alex{newLine}" +
                    $"\tHeight = 170,5{newLine}");
        }

        [Test]
        public void BeImmutable()
        {
            personPrinter
                .Excluding<Guid>()
                .PrintToString(person)
                .Should()
                .NotContain("Id");
            personPrinter
                .Excluding<int>()
                .PrintToString(person)
                .Should()
                .NotContain("Age")
                .And
                .Contain("Id");
        }

        [Test]
        public void ExcludeFieldWithType()
        {
            var result = personPrinter
                .Excluding<double>()
                .PrintToString(person);
            result
                .Should()
                .Be($"Person{newLine}" +
                    $"\tId = 00000000-0000-0000-0000-000000000000{newLine}" +
                    $"\tName = Alex{newLine}\tAge = 21{newLine}");
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
                .Be($"Person{newLine}" +
                    $"\tId = 00000000-0000-0000-0000-000000000000{newLine}" +
                    $"\tName = Alex{newLine}" +
                    $"\tAge = 21,00{newLine}" +
                    $"\tHeight = 170,5{newLine}");
        }

        [Test]
        public void UseCustomCulture_WhenSpecifiedForType()
        {
            var result = personPrinter
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .PrintToString(person);
            result.Should()
                .Be(
                    $"Person{newLine}" +
                    $"\tId = 00000000-0000-0000-0000-000000000000{newLine}" +
                    $"\tName = Alex{newLine}" +
                    $"\tAge = 21{newLine}" +
                    $"\tHeight = 170.5{newLine}");
        }

        [Test]
        public void UseCustomCulture_WhenSpecifiedForPrinter()
        {
            var result = personPrinter
                .UsingCulture(new CultureInfo("en-En"))
                .PrintToString(person);
            result.Should()
                .Be(
                    $"Person{newLine}" +
                    $"\tId = 00000000-0000-0000-0000-000000000000{newLine}" +
                    $"\tName = Alex{newLine}" +
                    $"\tAge = 21{newLine}" +
                    $"\tHeight = 170.5{newLine}");
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
                .Be($"Person{newLine}" +
                    $"\tId = 00000000-0000-0000-0000-000000000000{newLine}" +
                    $"\tName = ALEX{newLine}" +
                    $"\tAge = 21{newLine}" +
                    $"\tHeight = 170,5{newLine}");
        }

        [Test]
        public void TrimString_WhenSpecifiedForStringMember()
        {
            var result = personPrinter
                .Printing<string>().TrimmedToLength(2)
                .PrintToString(person);
            result.Should()
                .Be($"Person{newLine}" +
                    $"\tId = 00000000-0000-0000-0000-000000000000{newLine}" +
                    $"\tName = Al{newLine}" +
                    $"\tAge = 21{newLine}" +
                    $"\tHeight = 170,5{newLine}");
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
                .Be($"Person{newLine}" +
                    $"\tName = Alex{newLine}" +
                    $"\tAge = 21{newLine}" +
                    $"\tHeight = 170,5{newLine}");
        }

        [Test]
        public void PrintCycleReferenceMessage_WhenDefaultThrowOptionAndNoCustomSerializer()
        {
            var alex = new PersonWithFriend {Name = "Alex", Age = 19};
            var john = new PersonWithFriend {Name = "John", Age = 19};
            alex.Friend = john;
            john.Friend = alex;
            var result = personPrinter
                .Excluding<Guid>()
                .PrintToString(alex);
            result.Should()
                .Be($"PersonWithFriend{newLine}" +
                    $"\tFriend = PersonWithFriend{newLine}" +
                    $"\t\tFriend = cycle link detected on object with hashcode: {alex.GetHashCode()}{newLine}" +
                    $"\t\tName = John{newLine}" +
                    $"\t\tAge = 19{newLine}" +
                    $"\t\tHeight = 0{newLine}" +
                    $"\tName = Alex{newLine}" +
                    $"\tAge = 19{newLine}" +
                    $"\tHeight = 0{newLine}");
            Console.WriteLine(result);
        }

        [Test]
        public void Throw_WhenCycleMemberDetectedAndThrowOption()
        {
            var alex = new PersonWithFriend {Name = "Alex", Age = 19};
            var john = new PersonWithFriend {Name = "John", Age = 19};
            alex.Friend = john;
            john.Friend = alex;
            Assert.Throws<Exception>(() => personPrinter.ThrowingIfCycleReference(true).PrintToString(alex));
        }

        [Test]
        public void PrintCollections()
        {
            var container = new Container();
            var printer = ObjectPrinter.For<Container>();
            var result = printer
                .Excluding<Guid>()
                .Excluding<int>()
                .Excluding<double>()
                .Excluding(p => p.Age)
                .PrintToString(container);
            result.Should().Be($"Container{newLine}" +
                               $"\tPersons = {newLine}" +
                               $"\t{{{newLine}" +
                               $"\t\tPerson{newLine}" +
                               $"\t\t\tName = Alex{newLine}" +
                               $"\t\tPerson{newLine}" +
                               $"\t\t\tName = Riki{newLine}" +
                               $"\t\tPerson{newLine}" +
                               $"\t\t\tName = John{newLine}" +
                               $"\t}}{newLine}" +
                               $"\tNumbers = {newLine}" +
                               $"\t{{{newLine}" +
                               $"\t\t1{newLine}" +
                               $"\t\t2{newLine}" +
                               $"\t\t3{newLine}" +
                               $"\t\t4{newLine}" +
                               $"\t\t5{newLine}" +
                               $"\t\t6{newLine}" +
                               $"\t}}{newLine}");
            Console.WriteLine(result);
        }

        [Test]
        public void PrintDictionary()
        {
            var container = new Container();
            var printer = ObjectPrinter.For<Container>();
            var result = printer
                .Excluding<Guid>()
                .Excluding<int>()
                .Excluding<double>()
                .Excluding(p => p.Persons)
                .Excluding(p => p.Numbers)
                .PrintToString(container);
            result.Should()
                .Be($"Container{newLine}" +
                    $"\tAge = {newLine}" +
                    $"\t{{{newLine}" +
                    $"\t\tkey:Person{newLine}" +
                    $"\t\t\tName = Alex{newLine}" +
                    $"\t\tvalue:19{newLine}{newLine}" +
                    $"\t\tkey:Person{newLine}" +
                    $"\t\t\tName = Riki{newLine}" +
                    $"\t\tvalue:21{newLine}{newLine}" +
                    $"\t}}{newLine}");
            Console.WriteLine(result);
        }

        private class Person
        {
            public double Height;
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }

        private class PersonWithFriend : Person
        {
            public PersonWithFriend Friend { get; set; }
        }

        private class Container
        {
            public List<Person> Persons => new()
            {
                new Person {Name = "Alex"},
                new Person {Name = "Riki"},
                new Person {Name = "John"}
            };

            public int[] Numbers => new[] {1, 2, 3, 4, 5, 6};

            public Dictionary<Person, int> Age => new()
            {
                [new Person {Name = "Alex"}] = 19,
                [new Person {Name = "Riki"}] = 21
            };
        }
    }
}