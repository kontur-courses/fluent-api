using System;
using NUnit.Framework;
using FluentAssertions;
using System.Globalization;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person { Surname = "Foster", Name = "Alex", Age = 50, Height = 180, Weight = 83.65 };
        }


        [Test]
        public void PrintToString_PrintsDefaultString_WhenNoOptions()
        {
            var actualString = ObjectPrinter.For<Person>().PrintToString(person);

            var expectedSting = $"Person\r\n" +
                $"\tId = 00000000-0000-0000-0000-000000000000\r\n" +
                $"\tSurname = Foster\r\n" +
                $"\tName = Alex\r\n" +
                $"\tAge = 50\r\n" +
                $"\tHeight = 180\r\n" +
                $"\tWeight = 83,65\r\n" +
                $"\tCar = null\r\n" +
                $"\tVisitedCountries = null\r\n" +
                $"\tCitizenships = null\r\n" +
                $"\tIncomeTaxByYear = null\r\n" +
                $"\tChildren = null\r\n" +
                $"\tParents = null\r\n";

            expectedSting.Should().Be(actualString);
        }

        [Test]
        public void PrintToString_DoesNotPrintPropertyWithExcludedType()
        {
            var actualString = ObjectPrinter.For<Person>()
               .Excluding<Guid>()
               .PrintToString(person);

            Console.WriteLine(actualString);

            actualString.Should().NotContain("Id");
        }

        [Test]
        public void PrintToString_DoesNotPrintExcludedProperty()
        {
            var actualString = ObjectPrinter.For<Person>()
               .Excluding(p => p.Height)
               .PrintToString(person);

            Console.WriteLine(actualString);

            actualString.Should().NotContain("Height");
        }

        [Test]
        public void PrintToString_PrintsPropertiesWithTypeSerializationOption()
        {
            var actualString = ObjectPrinter.For<Person>()
                .Printing<string>().Using(i => i.ToUpper())
                .PrintToString(person);

            Console.WriteLine(actualString);

            actualString.Should().Contain(person.Surname.ToUpper()).And.Contain(person.Name.ToUpper());
        }

        [Test]
        public void PrintToString_PrintsNumericalTypeWithSpecifiedCulture()
        {
            var culture = new CultureInfo("en");

            var actualString = ObjectPrinter.For<Person>()
                .Printing<double>().Using(culture)
                .PrintToString(person);

            Console.WriteLine(actualString);

            actualString.Should().Contain($"\tWeight = {person.Weight.ToString(null, culture)}\r\n");
        }

        [Test]
        public void PrintToString_PrintsPropertyWithSerializationOption()
        {
            var actualString = ObjectPrinter.For<Person>()
                .Printing(p => p.Age).Using(age => $"{age} years old")
                .PrintToString(person);

            Console.WriteLine(actualString);

            actualString.Should().Contain($"\tAge = {person.Age} years old\r\n");
        }

        [Test]
        public void PrintToString_PrintsTrimmedStringProperty()
        {
            var maxLength = 2;

            var actualString = ObjectPrinter.For<Person>()
                 .Printing(p => p.Name).TrimmedToLength(maxLength)
                 .PrintToString(person);

            Console.WriteLine(actualString);

            actualString.Should().Contain($"\tName = {person.Name.Substring(0, maxLength)}\r\n");
        }

        [Test]
        public void PrintToString_InformsAboutCycledReference_WnenCycledReferenceExists()
        {
            var car = new Car { Id = 777 };
            person.Car = car;
            car.Owner = person;

            var actualString = ObjectPrinter.For<Person>().PrintToString(person);

            Console.WriteLine(actualString);

            actualString.Should().Contain($"Cycled reference detected. Object <{person.GetType().Name}> doesn't printed\r\n");
        }

        [Test]
        public void PrintToString_DoesNotThrowException_OnEmptyObject()
        {
            Action action = () => ObjectPrinter.For<Person>().PrintToString(new Person());

            action.Should().NotThrow<Exception>();
        }

        [Test]
        public void PrintToString_PrintsPropertyWithMultipleConfigs()
        {
            var maxLength = 3;

            var actualString = ObjectPrinter.For<Person>()
                .Printing(x => x.Name).Using(x => x.ToUpper())
                .Printing(x => x.Name).TrimmedToLength(maxLength)
                .PrintToString(person);

            Console.WriteLine(actualString);

            actualString.Should().Contain(person.Name.Substring(0, maxLength).ToUpper());
        }

        [Test]
        public void PrintToString_PrintsPropertyWithIntersectingConfigs()
        {
            var maxLength = 3;

            var actualString = ObjectPrinter.For<Person>()
                .Printing(x => x.Name).Using(x => x.ToUpper())
                .Printing<string>().TrimmedToLength(maxLength)
                .PrintToString(person);

            Console.WriteLine(actualString);

            actualString
                .Should()
                .Contain($"\tSurname = {person.Surname.Substring(0, maxLength)}\r\n")
                .And
                .Contain($"\tName = {person.Name.Substring(0, maxLength).ToUpper()}\r\n");
        }

        [Test]
        public void PrintToString_PrintsTypeWithMultipleConfigs()
        {
            var actualString = ObjectPrinter.For<Person>()
                .Printing<string>().Using(x => x.ToUpper())
                .Printing<string>().TrimmedToLength(4)
                .PrintToString(person);

            Console.WriteLine(actualString);

            actualString.Should().Contain("\tSurname = FOST\r\n").And.Contain("\tName = ALEX\r\n");
        }

        [Test]
        public void PrintToString_PrintsListSimpleTypeProperty()
        {
            person.VisitedCountries = new List<string>() { "Cyprus", "Austria", "Hungary" };

            var actualString = ObjectPrinter.For<Person>().PrintToString(person);

            Console.WriteLine(actualString);

            actualString.Should().Contain("\tVisitedCountries = \n\t\t[ \n\t\tCyprus\n\t\tAustria\n\t\tHungary\n\t\t]\r\n");
        }

        [Test]
        public void PrintToString_PrintsListComplexTypeProperty()
        {
            person.Children = new List<Person>()
            {
                new Person { Surname = "Foster", Name = "George", Age = 25 },
                new Person { Surname = "Foster", Name = "Anna", Age = 20 }
            };

            var actualString = ObjectPrinter.For<Person>().PrintToString(person);

            Console.WriteLine(actualString);

            actualString
                .Should()
                .Contain("\t\t\t\tSurname = Foster\r\n\t\t\t\tName = George\r\n")
                .And
                .Contain("\t\t\t\tSurname = Foster\r\n\t\t\t\tName = Anna\r\n");
        }

        [Test]
        public void PrintToString_PrintsArraySimpleTypeProperty()
        {
            person.Citizenships = new string[] { "Russia", "Israel" };

            var actualString = ObjectPrinter.For<Person>().PrintToString(person);

            Console.WriteLine(actualString);

            actualString.Should().Contain("\tCitizenships = \n\t\t[ \n\t\tRussia\n\t\tIsrael\n\t\t]\r\n");
        }

        [Test]
        public void PrintToString_PrintsDictionarySimpleTypeProperty()
        {
            person.IncomeTaxByYear = new Dictionary<int, int>
            {
                {2019, 111111 },
                {2020, 222222 },
                {2021, 333333 },
            };

            var actualString = ObjectPrinter.For<Person>().PrintToString(person);

            Console.WriteLine(actualString);

            actualString.Should().Contain("\tIncomeTaxByYear = \n\t\t[ 2019 ] = 111111\n\t\t[ 2020 ] = 222222\n\t\t[ 2021 ] = 333333\r\n");
        }

        [Test]
        public void PrintToString_PrintsDictionaryComplexTypeProperty()
        {
            person.Parents = new Dictionary<string, Person>
            {
                {"Parent 1", new Person { Surname = "Foster", Name = "Нarry", Age = 75 } },
                {"Parent 2", new Person { Surname = "Foster", Name = "Ginny", Age = 50 } }
            };

            var actualString = ObjectPrinter.For<Person>().PrintToString(person);

            Console.WriteLine(actualString);

            actualString
                .Should()
                .Contain("\t\t\tSurname = Foster\r\n\t\t\tName = Нarry\r\n")
                .And
                .Contain("\t\t\tSurname = Foster\r\n\t\t\tName = Ginny\r\n");
        }

    }
}
