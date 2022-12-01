using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterTest
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person
            {
                Name = "Ivan Ivanov",
                Age = 19,
                Height = 190.50,
                Weight = 80
            };
        }

        [Test]
        public void ObjectPrinter_ShouldExcludeByType()
        {
            var result = ObjectPrinter.For<Person>()
                .Excluding<string>()
                .PrintToString(person);

            result.Should().NotContain(nameof(Person.Name) + " = ");
        }

        [Test]
        public void ObjectPrinter_ShouldExcludeProperty()
        {
            var result = ObjectPrinter.For<Person>()
                .Excluding(x => x.Id)
                .PrintToString(person);

            var notExcepted = nameof(Person.Id) + " = " + person.Id + Environment.NewLine;
            result.Should().NotContain(notExcepted);
        }

        [Test]
        public void ObjectPrinter_ShouldProvideCustomSerialization_ForType()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(x => x.ToString("f0"))
                .PrintToString(person);
            var heightExcepted = nameof(Person.Height) + " = " + person.Height.ToString("f0") + Environment.NewLine;
            var weightExcepted = nameof(Person.Weight) + " = " + person.Weight.ToString("f0") + Environment.NewLine;
            result.Should().Contain(heightExcepted).And.Contain(weightExcepted);
        }

        [Test]
        public void ObjectPrinter_ShouldProvideCustomSerialization_ForProperty()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing(t => t.Weight)
                .Using(x => x.ToString("f0"))
                .PrintToString(person);
            var excepted = nameof(Person.Weight) + " = " + person.Weight.ToString("f0") + Environment.NewLine;
            ;
            result.Should().Contain(excepted);
        }

        [Test]
        public void ObjectPrinter_ShouldProvideCulture_ForIFormattableTypes()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(CultureInfo.InvariantCulture)
                .PrintToString(person);
            var excepted = nameof(Person.Weight) + " = " + person.Weight.ToString("f0") + Environment.NewLine;

            result.Should().Contain(excepted);
        }

        [Test]
        public void ObjectPrinter_ShouldProvideTrimForStrings()
        {
            const int length = 5;
            var result = ObjectPrinter.For<Person>()
                .Printing<string>()
                .TrimmedToLength(5)
                .PrintToString(person);
            var excepted = nameof(Person.Name) + " = " + person.Name[..length] + Environment.NewLine;
            result.Should().Contain(excepted);
        }


        [Test]
        public void ObjectPrinter_ShouldWork_WhenReferenceCycles()
        {
            const string excepted = "Обнаружен цикл! Объект будет пропущен.";
            person.Parents = new[] { person };
            person.Children = new List<Person> { person };
            var result = ObjectPrinter.For<Person>().PrintToString(person);
            result.Should().Contain(excepted);
        }

        [Test]
        public void ObjectPrinter_ShouldPrint_WhenCollections()
        {
            const string excepted =
                "Person\r\n\tWeight = 80\r\n\tParents = Person[] {\r\n\t\tPerson\r\n\t\t\tWeight = 0\r\n\t\t\tParents = null\r\n\t\tPerson\r\n\t\t\tWeight = 0\r\n\t\t\tParents = null\r\n\t}\r\n";
            person.Parents = new[]
            {
                new Person(),
                new Person()
            };
            var result = ObjectPrinter.For<Person>()
                .Excluding(t => t.OtherInfo)
                .Excluding(t => t.Children)
                .Excluding<Guid>()
                .Excluding<double>()
                .Excluding<bool>()
                .Excluding<string>()
                .Excluding<int>()
                .PrintToString(person);
            Console.WriteLine(result);
            result.Should().Be(excepted);
        }


        [Test]
        public void ObjectPrinter_ShouldPrint_WhenDictionaries()
        {
            const string expected =
                "Person\r\n\tWeight = 80\r\n\tOtherInfo = Dictionary`2 {\r\n\t\tPassport = 1234 123456\r\n\t\tUniversity = MSU\r\n\t}\r\n";
            person.OtherInfo = new Dictionary<string, string>
            {
                { "Passport", "1234 123456" },
                { "University", "MSU" }
            };
            var result = ObjectPrinter.For<Person>()
                .Excluding(t => t.Parents)
                .Excluding(t => t.Children)
                .Excluding<Guid>()
                .Excluding<double>()
                .Excluding<bool>()
                .Excluding<string>()
                .Excluding<int>()
                .PrintToString(person);
            result.Should().Be(expected);
        }

        [Test]
        public void Demo()
        {
            var ivan = new Person
            {
                Name = "Ivan Ivanov", 
                Height = 190,
                Weight = 801,
                Age = 30
            };
            ivan.Children = new List<Person>()
            {
                new()
                {
                    Age = 7,
                    Height = 160,
                    Weight = 40,
                    Name = "Petya",
                    Parents = new []{ ivan }
                },
                new()
                {
                    Age = 7,
                    Height = 160,
                    Weight = 40,
                    Name = "Vanya",
                    Parents = new []{ ivan }
                }
            };
            var result = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .Printing<double>().Using(t => t.ToString("f0"))
                .Printing(p => p.Age).Using(t => $"{t} years")
                .Printing(p => p.Height).Using(t => $"{t} cm")
                .Printing(p => p.Weight).Using(t => $"{t} kg")
                .PrintToString(ivan);

            Console.WriteLine(result);
        }
    }
}