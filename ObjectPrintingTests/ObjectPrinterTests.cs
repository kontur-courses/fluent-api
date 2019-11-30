using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrintingTests.TestsElements;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        [Test]
        public void PrintToStringDefaultPerson()
        {
            var person = new Person();
            var printer = ObjectPrinter.For<Person>();

            var printingString = printer.PrintToString(person);

            foreach (var propertyInfo in typeof(Person).GetProperties())
                printingString.Should().Contain(propertyInfo.Name);
        }

        [Test]
        public void PrintToStringPersonWithAllExcludingTypes()
        {
            var person = new Person {Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>()
                .Excluding<int>()
                .Excluding<Guid>()
                .Excluding<double>()
                .Excluding<Person>();

            var printingString = printer.PrintToString(person);

            foreach (var propertyInfo in typeof(Person).GetProperties())
                printingString.Should().NotContain(propertyInfo.Name);
        }

        [Test]
        public void PrintToStringPersonWithExcludingClassTypes()
        {
            var person = new Person {Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Excluding<string>()
                .Excluding<Person>();

            var printingString = printer.PrintToString(person);

            printingString.Should().NotContain("Name");
            printingString.Should().NotContain("Parent");
        }

        [Test]
        public void PrintToStringPersonWithExcludingStructTypes()
        {
            var person = new Person {Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>()
                .Excluding<double>()
                .Excluding<Guid>();

            var printingString = printer.PrintToString(person);

            printingString.Should().NotContain("Age");
            printingString.Should().NotContain("Height");
            printingString.Should().NotContain("Id");
        }

        [Test]
        public void PrintToStringPersonWithExcludingClassProperty()
        {
            var person = new Person {Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Name)
                .Excluding(p => p.Parent);

            var printingString = printer.PrintToString(person);

            printingString.Should().NotContain("Name");
            printingString.Should().NotContain("Parent");
        }

        [Test]
        public void PrintToStringPersonWithExcludingStructProperty()
        {
            var person = new Person {Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age)
                .Excluding(p => p.Height)
                .Excluding(p => p.Id);

            var printingString = printer.PrintToString(person);

            printingString.Should().NotContain("Age");
            printingString.Should().NotContain("Height");
            printingString.Should().NotContain("Id");
        }

        [Test]
        public void PrintToStringPersonWithCustomSerializationForType()
        {
            var person = new Person {Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Serialize<string>().Using(s => s.ToUpper());

            var printingString = printer.PrintToString(person);

            printingString.Should().Contain(person.Name.ToUpper());
        }

        [Test]
        public void PrintToStringPersonWithCustomSerializationForProperty()
        {
            var person = new Person {Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Serialize(p => p.Name).Using(s => $"Name of this = {s.ToUpper()}");

            var printingString = printer.PrintToString(person);

            printingString.Should().Contain($"Name of this = {person.Name.ToUpper()}");
        }

        [TestCase("ru-RU", 2.5, "2,5")]
        [TestCase("en-GB", 2.5, "2.5")]
        public void PrintToStringPersonWithCertainCultureForNumberTypes(string culture, double number, string expectedSerialiseNumber)
        {
            var person = new Person {Name = "Alex", Age = 18, Height = number};
            var printer = ObjectPrinter.For<Person>()
                .Serialize<double>().WithCulture(new CultureInfo(culture));

            var printingString = printer.PrintToString(person);

            printingString.Should().Contain(expectedSerialiseNumber);
        }

        [Test]
        public void PrintToStringPersonWithCutForStringTypes()
        {
            var person = new Person {Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Serialize(p => p.Name).Cut(3);

            var printingString = printer.PrintToString(person);

            printingString.Should().Contain("Ale");
        }
        
        [Test]
        public void PrintToStringPersonWithCut_When_CountSymbolsGreaterThanStringLength()
        {
            var person = new Person {Name = "Alex", Age = 18};
            var printer = ObjectPrinter.For<Person>()
                .Serialize(p => p.Name).Cut(person.Name.Length + 1);

            Action act = () => printer.PrintToString(person);

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void PrintToStringPersonWithCyclicDependence()
        {
            var person1 = new Person();
            var person2 = new Person();
            person1.Parent = person2.Parent;
            person2.Parent = person1.Parent;
            var printer = ObjectPrinter.For<Person>();

            Action act = () => printer.PrintToString(person1);

            act.Should().NotThrow();
        }

        [Test]
        public void PrintOnlyFinalType()
        {
            int @int = default;
            double @double = default;

            var printingInt = @int.PrintToString();
            var printingDouble = @double.PrintToString();

            printingInt.Should().BeEquivalentTo(@int + Environment.NewLine);
            printingDouble.Should().BeEquivalentTo(@double + Environment.NewLine);
        }

        [Test]
        public void PrintToStringListOfPersons()
        {
            var listOfPersons = new List<Person>
            {
                new Person() {Name = "A"},
                new Person() {Name = "B"},
                new Person() {Name = "C"},
            };

            var printingList = listOfPersons.PrintToString(c => c
                .Excluding<Guid>()
                .Excluding<double>()
                .Excluding<int>());

            foreach (var person in listOfPersons)
            {
                printingList.Should().Contain(person.Name);
            }
        }
        
        [Test]
        public void PrintToStringDictionaryOfPersons()
        {
            var dictionaryOfPersons = new Dictionary<string,Person>(){
                ["1"] = new Person() {Name = "A"},
                ["2"] = new Person() {Name = "B"},
                ["3"] = new Person() {Name = "C"},
            };

            var printingDictionary = dictionaryOfPersons.PrintToString(c => c
                .Excluding<Guid>()
                .Excluding<double>()
                .Excluding<int>());

            foreach (var dictionaryElement in dictionaryOfPersons)
            {
                printingDictionary.Should().Contain(dictionaryElement.Key);
                printingDictionary.Should().Contain(dictionaryElement.Value.Name);
            }
        }
    }
}