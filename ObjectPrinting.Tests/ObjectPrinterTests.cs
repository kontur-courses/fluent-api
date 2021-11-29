using System;
using System.ComponentModel;
using FluentAssertions;
using NUnit.Framework;

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
            person = new()
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
        }

        [Test]
        public void PrintToString_PrintPublicPropertiesAndFields()
        {
            personPrinter.PrintToString(person)
                .Should()
                .Be(defaultPersonSerialization);
        }

        [Test]
        public void Excluding_ExcludePropertyWithType_WhenItExists()
        {
            var result = personPrinter
                .Excluding<Guid>()
                .PrintToString(person);
            result.Should()
                .Be($"Person{newLine}\tName = Alex{newLine}\tAge = 21{newLine}\tHeight = 170,5{newLine}");
        }

        [Test]
        public void Excluding_ExcludeFieldWithType()
        {
            var result = personPrinter
                .Excluding<double>();
        }

        [Test]
        public void Excluding_ExcludeNothing_WhenObjectDoesNotContainsType()
        {
            var result = personPrinter
                .Excluding<float>()
                .PrintToString(person);
            result.Should()
                .Be(defaultPersonSerialization);
        }

        private class Person
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public double Height;
        }
    }
}