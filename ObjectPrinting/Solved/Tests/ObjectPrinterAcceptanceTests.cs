using FluentAssertions;
using System;
using System.Globalization;
using NUnit.Framework;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace ObjectPrinting.Solved.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private string newLine = Environment.NewLine;
        private Person person;

        [SetUp]
        public void PersonInitialization()
        {
            person = new Person()
            {
                Name = "Vladimir Vladimirovich",
                Age = 21,
                Height = 190.5,
                Id = new Guid(),
                Health = 100,
                DateOfBirth = new DateTime(2000, 1, 18),
                Parent = new Person() { Name = "Vladimir", Age = 45, Health = 1000 }
            };
        }

        [Test]
        public void ExcludePropertiesByType()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Excluding<int>();
            var resProperty = new List<string>()
            {
                "Person",
                "\tId = Guid",
                "\tName = Vladimir Vladimirovich",
                "\tHeight = 190,5",
                "\tDateOfBirth = 18.01.2000 0:00:00",
                "\tParent = Person",
                "\t\tId = Guid",
                "\t\tName = Vladimir",
                "\t\tHeight = 0",
                "\t\tDateOfBirth = 01.01.0001 0:00:00",
                "\t\tParent = null"
            };

            var res = printer.PrintToString(person);

            res.Should().Be(string.Join(newLine, resProperty));
        }


        [Test]
        public void PruneStringProperty()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Printing(person => person.Name).TrimmedToLength(3);
            var resProperty = new List<string>()
            {
                "Person",
                "\tId = Guid",
                "\tName = Vla",
                "\tHeight = 190,5",
                "\tAge = 21",
                "\tHealth = 100",
                "\tDateOfBirth = 18.01.2000 0:00:00",
                "\tParent = Person",
                "\t\tId = Guid",
                "\t\tName = Vladimir",
                "\t\tHeight = 0",
                "\t\tAge = 45",
                "\t\tHealth = 1000",
                "\t\tDateOfBirth = 01.01.0001 0:00:00",
                "\t\tParent = null"
            };

            var res = printer.PrintToString(person);

            res.Should().Be(string.Join(newLine, resProperty));
        }

        [Test]
        public void AlternativePrintForProp()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Printing(person => person.Health).Using(health => $"{health}/100");
            var resProperty = new List<string>()
            {
                "Person",
                "\tId = Guid",
                "\tName = Vladimir Vladimirovich",
                "\tHeight = 190,5",
                "\tAge = 21",
                "\tHealth = 100/100",
                "\tDateOfBirth = 18.01.2000 0:00:00",
                "\tParent = Person",
                "\t\tId = Guid",
                "\t\tName = Vladimir",
                "\t\tHeight = 0",
                "\t\tAge = 45",
                "\t\tHealth = 1000",
                "\t\tDateOfBirth = 01.01.0001 0:00:00",
                "\t\tParent = null"
            };

            var res = printer.PrintToString(person);

            res.Should().Be(string.Join(newLine, resProperty));
        }

        [Test]
        public void AlternativePrintForType()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Printing<int>().Using(i => $"(целое число){i}");
            var resProperty = new List<string>()
            {
                "Person",
                "\tId = Guid",
                "\tName = Vladimir Vladimirovich",
                "\tHeight = 190,5",
                "\tAge = (целое число)21",
                "\tHealth = (целое число)100",
                "\tDateOfBirth = 18.01.2000 0:00:00",
                "\tParent = Person",
                "\t\tId = Guid",
                "\t\tName = Vladimir",
                "\t\tHeight = 0",
                "\t\tAge = (целое число)45",
                "\t\tHealth = (целое число)1000",
                "\t\tDateOfBirth = 01.01.0001 0:00:00",
                "\t\tParent = null"
            };

            var res = printer.PrintToString(person);

            res.Should().Be(string.Join(newLine, resProperty));
        }

        [Test]
        public void ChangeCultureForDouble()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Printing<double>().UsingCulture(CultureInfo.InvariantCulture);
            var resProperty = new List<string>()
            {
                "Person",
                "\tId = Guid",
                "\tName = Vladimir Vladimirovich",
                "\tHeight = 190.5",
                "\tAge = 21",
                "\tHealth = 100",
                "\tDateOfBirth = 18.01.2000 0:00:00",
                "\tParent = Person",
                "\t\tId = Guid",
                "\t\tName = Vladimir",
                "\t\tHeight = 0",
                "\t\tAge = 45",
                "\t\tHealth = 1000",
                "\t\tDateOfBirth = 01.01.0001 0:00:00",
                "\t\tParent = null"
            };

            var res = printer.PrintToString(person);

            res.Should().Be(string.Join(newLine, resProperty));
        }

        [Test]
        public void ChangeCultureForDateTime()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Printing<DateTime>().UsingCulture(CultureInfo.InvariantCulture, "dd/MM/yyyy");
            var resProperty = new List<string>()
            {
                "Person",
                "\tId = Guid",
                "\tName = Vladimir Vladimirovich",
                "\tHeight = 190,5",
                "\tAge = 21",
                "\tHealth = 100",
                "\tDateOfBirth = 18/01/2000",
                "\tParent = Person",
                "\t\tId = Guid",
                "\t\tName = Vladimir",
                "\t\tHeight = 0",
                "\t\tAge = 45",
                "\t\tHealth = 1000",
                "\t\tDateOfBirth = 01/01/0001",
                "\t\tParent = null"
            };

            var res = printer.PrintToString(person);
            Console.WriteLine(res);

            res.Should().Be(string.Join(newLine, resProperty));
        }

        [Test]
        public void ExcludeProp()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(person => person.Age);
            var resProperty = new List<string>()
            {
                "Person",
                "\tId = Guid",
                "\tName = Vladimir Vladimirovich",
                "\tHeight = 190,5",
                "\tHealth = 100",
                "\tDateOfBirth = 18.01.2000 0:00:00",
                "\tParent = Person",
                "\t\tId = Guid",
                "\t\tName = Vladimir",
                "\t\tHeight = 0",
                "\t\tAge = 45",
                "\t\tHealth = 1000",
                "\t\tDateOfBirth = 01.01.0001 0:00:00",
                "\t\tParent = null"
            };

            var res = printer.PrintToString(person);

            res.Should().Be(string.Join(newLine, resProperty));
        }

        [Test]
        public void ExcludeInnerProp()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(person => person.Parent.Age);
            var resProperty = new List<string>()
            {
                "Person",
                "\tId = Guid",
                "\tName = Vladimir Vladimirovich",
                "\tHeight = 190,5",
                "\tAge = 21",
                "\tHealth = 100",
                "\tDateOfBirth = 18.01.2000 0:00:00",
                "\tParent = Person",
                "\t\tId = Guid",
                "\t\tName = Vladimir",
                "\t\tHeight = 0",
                "\t\tHealth = 1000",
                "\t\tDateOfBirth = 01.01.0001 0:00:00",
                "\t\tParent = null"
            };

            var res = printer.PrintToString(person);
            Console.WriteLine(res);

            res.Should().Be(string.Join(newLine, resProperty));
        }

        [Test]
        public void PrintingWithoutAdditionalParameters()
        {
            var printer = ObjectPrinter.For<Person>();
            var resProperty = new List<string>()
            {
                "Person",
                "\tId = Guid",
                "\tName = Vladimir Vladimirovich",
                "\tHeight = 190,5",
                "\tAge = 21",
                "\tHealth = 100",
                "\tDateOfBirth = 18.01.2000 0:00:00",
                "\tParent = Person",
                "\t\tId = Guid",
                "\t\tName = Vladimir",
                "\t\tHeight = 0",
                "\t\tAge = 45",
                "\t\tHealth = 1000",
                "\t\tDateOfBirth = 01.01.0001 0:00:00",
                "\t\tParent = null"
            };

            var res = printer.PrintToString(person);
            Console.WriteLine(res);

            res.Should().Be(string.Join(newLine, resProperty));
        }

        [Test]
        public void ExcludingTypeAndAlternativePrintForType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>()
                .Printing<string>().Using(str => str.ToUpper());
            var resProperty = new List<string>()
            {
                "Person",
                "\tId = Guid",
                "\tHeight = 190,5",
                "\tAge = 21",
                "\tHealth = 100",
                "\tDateOfBirth = 18.01.2000 0:00:00",
                "\tParent = Person",
                "\t\tId = Guid",
                "\t\tHeight = 0",
                "\t\tAge = 45",
                "\t\tHealth = 1000",
                "\t\tDateOfBirth = 01.01.0001 0:00:00",
                "\t\tParent = null"
            };
        }

        [Test]
        public void AlternativePrintForPropAndExcludingProp()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(person => person.Name).Using(str => str.ToUpper())
                .Excluding(person => person.Name);
            var resProperty = new List<string>()
            {
                "Person",
                "\tId = Guid",
                "\tHeight = 190,5",
                "\tAge = 21",
                "\tHealth = 100",
                "\tDateOfBirth = 18.01.2000 0:00:00",
                "\tParent = Person",
                "\t\tId = Guid",
                "\t\tName = Vladimir",
                "\t\tHeight = 0",
                "\t\tAge = 45",
                "\t\tHealth = 1000",
                "\t\tDateOfBirth = 01.01.0001 0:00:00",
                "\t\tParent = null"
            };

            var res = printer.PrintToString(person);
            Console.WriteLine(res);

            res.Should().Be(string.Join(newLine, resProperty));
        }

        [Test]
        public void ExcludingTypeAndAlternativePrintForProp()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>()
                .Printing(person => person.Name).Using(str => str.ToUpper());
            var resProperty = new List<string>()
            {
                "Person",
                "\tId = Guid",
                "\tHeight = 190,5",
                "\tAge = 21",
                "\tHealth = 100",
                "\tDateOfBirth = 18.01.2000 0:00:00",
                "\tParent = Person",
                "\t\tId = Guid",
                "\t\tHeight = 0",
                "\t\tAge = 45",
                "\t\tHealth = 1000",
                "\t\tDateOfBirth = 01.01.0001 0:00:00",
                "\t\tParent = null"
            };

            var res = printer.PrintToString(person);
            Console.WriteLine(res);

            res.Should().Be(string.Join(newLine, resProperty));
        }

        [Test]
        public void ExcludingPropAndAlternativePrintForType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(person => person.Name)
                .Printing<string>().Using(str => str.ToUpper());
            var resProperty = new List<string>()
            {
                "Person",
                "\tId = Guid",
                "\tHeight = 190,5",
                "\tAge = 21",
                "\tHealth = 100",
                "\tDateOfBirth = 18.01.2000 0:00:00",
                "\tParent = Person",
                "\t\tId = Guid",
                "\t\tName = VLADIMIR",
                "\t\tHeight = 0",
                "\t\tAge = 45",
                "\t\tHealth = 1000",
                "\t\tDateOfBirth = 01.01.0001 0:00:00",
                "\t\tParent = null"
            };

            var res = printer.PrintToString(person);
            Console.WriteLine(res);

            res.Should().Be(string.Join(newLine, resProperty));
        }

        [Test]
        public void AlternativePrintForPropAndAlternativePrintForType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(person => person.Name).Using(name => $"(Псевдоним){name}")
                .Printing<string>().Using(str => str.ToUpper());
            var resProperty = new List<string>()
            {
                "Person",
                "\tId = Guid",
                "\tName = (Псевдоним)Vladimir Vladimirovich",
                "\tHeight = 190,5",
                "\tAge = 21",
                "\tHealth = 100",
                "\tDateOfBirth = 18.01.2000 0:00:00",
                "\tParent = Person",
                "\t\tId = Guid",
                "\t\tName = VLADIMIR",
                "\t\tHeight = 0",
                "\t\tAge = 45",
                "\t\tHealth = 1000",
                "\t\tDateOfBirth = 01.01.0001 0:00:00",
                "\t\tParent = null"
            };

            var res = printer.PrintToString(person);
            Console.WriteLine(res);

            res.Should().Be(string.Join(newLine, resProperty));
        }

        [Test]
        public void SpecialPrintingPropAndExcludingType()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing(person => person.Name).Using(str => str.ToUpper())
                .Excluding<string>();
            var resProperty = new List<string>()
            {
                "Person",
                "\tId = Guid",
                "\tHeight = 190,5",
                "\tAge = 21",
                "\tHealth = 100",
                "\tDateOfBirth = 18.01.2000 0:00:00",
                "\tParent = Person",
                "\t\tId = Guid",
                "\t\tHeight = 0",
                "\t\tAge = 45",
                "\t\tHealth = 1000",
                "\t\tDateOfBirth = 01.01.0001 0:00:00",
                "\t\tParent = null"
            };

            var res = printer.PrintToString(person);
            Console.WriteLine(res);

            res.Should().Be(string.Join(newLine, resProperty));
        }

        [Test]
        public void ExcludeAllProp()
        {
            var printer = ObjectPrinter.For<Person>()
                .Excluding(person => person.Id)
                .Excluding<Person>()
                .Excluding<int>()
                .Excluding<double>()
                .Excluding<DateTime>()
                .Excluding<string>();

            var res = printer.PrintToString(person);

            res.Should().Be("Person");
        }

        [Test]
        public void ShouldNotThrowExceptionWhenCircularLinks()
        {
            var parent = new Person()
            {
                Name = "Father",
                Parent = person
            };
            person.Parent = parent;
            var printer = ObjectPrinter.For<Person>()
                .Excluding(person => person.Parent.Age);

            Action act = () => printer.PrintToString(person, 10);

            act.Should().NotThrow();
        }
    }
}