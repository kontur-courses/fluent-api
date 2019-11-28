using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        [TestCase(1, TestName = "OnOneElement")]
        [TestCase(1, 2, 3, 4, 5, TestName = "OnManyElements")]
        public void GetStringRepresentation_ReturnsCorrectAnswer_OnCollections(params int[] elements)
        {
            var printedLine = elements.PrintToString();
            foreach (var element in elements)
            {
                printedLine.Should().Contain(element.ToString());
            }
        }

        [TestCase("string", 1, "string2", 2)]
        [TestCase("TEST", 2, "TEST2", 3)]
        public void GetStringRepresentation_ReturnsCorrectAnswer_OnDictionaries(string firstKey, int firstValue, string secondKey, int secondValue)
        {
            var dict = new Dictionary<string, int>();
            dict[firstKey] = firstValue;
            dict[secondKey] = secondValue;
            var printedLine = dict.PrintToString();
            printedLine.Should().Contain(firstKey);
            printedLine.Should().Contain(firstValue.ToString());
            printedLine.Should().Contain(secondKey);
            printedLine.Should().Contain(secondValue.ToString());
        }

        [Test]
        public void GetStringRepresentation_ReturnsCorrectAnswer_WhenNoCustomConfiguration()
        {
            var amountOfTries = 5;
            var nameLength = 5;

            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            for (var i = 0; i < amountOfTries; i++)
            {
                var guid = Guid.NewGuid();
                var name = new string(Enumerable.Repeat(chars, nameLength).Select(x => x[random.Next(nameLength)]).ToArray());
                var height = random.NextDouble();
                var age = random.Next();
                var person = new Person() { Age = age, Height = height, Id = guid, Name = name };
                var printedLine = person.PrintToString();

                printedLine.Should().Contain(name);
                printedLine.Should().Contain(height.ToString());
                printedLine.Should().Contain(age.ToString());
                printedLine.Should().Contain(guid.ToString());
            }
        }

        private class CircularReferenceExampleClass
        {
            public CircularReferenceExampleClass otherObject { get; set; }
        }

        [Test]
        public void GetStringRepresentation_ThrowsFormatException_OnCircularReference()
        {
            var firstObject = new CircularReferenceExampleClass();
            var secondObject = new CircularReferenceExampleClass();
            firstObject.otherObject = secondObject;
            secondObject.otherObject = firstObject;
            Action action = () => firstObject.PrintToString();
            action.Should().Throw<FormatException>();
        }

        [TestCase(1, 7, 8)]
        [TestCase(0, 0, 0)]
        [TestCase(0, -5, -5)]
        public void GetStringRepresentation_ReturnsCorrectAnswer_OnCustomSerializationByProperty(int value, int offset, int expectedAnswer)
        {
            var person = new Person();
            person.Age = value;
            var printedLine = person.PrintToString(settings =>
                settings.For(x => x.Age).WithSerialization(x => (x + offset).ToString()));
            printedLine.Should().Contain(expectedAnswer.ToString());
        }

        [TestCase(1, 7, 8)]
        [TestCase(0, 0, 0)]
        [TestCase(0, -5, -5)]
        public void GetStringRepresentation_ReturnsCorrectAnswer_OnCustomSerializationByType(int value, int offset, int expectedAnswer)
        {
            var person = new Person();
            person.Age = value;
            var printedLine = person.PrintToString(settings =>
                settings.For<int>().WithSerialization(x => (x + offset).ToString()));
            printedLine.Should().Contain(expectedAnswer.ToString());
        }

        [TestCase(1)]
        [TestCase(-5)]
        public void GetStringRepresentation_ReturnsCorrectAnswer_OnExcludingByProperty(int value)
        {
            var person = new Person();
            person.Age = value;
            var printedLine = person.PrintToString(settings =>
                settings.Excluding(x => x.Age));
            printedLine.Should().NotContain(value.ToString());
        }

        [TestCase(1)]
        [TestCase(-5)]
        public void GetStringRepresentation_ReturnsCorrectAnswer_OnExcludingByType(int value)
        {
            var person = new Person();
            person.Age = value;
            var printedLine = person.PrintToString(settings =>
                settings.Excluding<int>());
            printedLine.Should().NotContain(value.ToString());
        }

        [TestCase("TestValue", 5)]
        [TestCase("12", 1)]
        public void GetStringRepresentation_ReturnsCorrectAnswer_OnTrim(string valueToTrim, int length)
        {
            var person = new Person();
            person.Name = valueToTrim;
            var printedLine = person.PrintToString(settings =>
                settings.For(x => x.Name).Trim(length));
            printedLine.Should().Contain(valueToTrim.Substring(0, length));
            printedLine.Should().NotContain(valueToTrim);
        }

        [Test]
        public void GetStringRepresentation_ReturnsCorrectAnswer_OnTrimValueLongerThanString()
        {
            var person = new Person();
            person.Name = "TEST";
            var printedLine = person.PrintToString(settings =>
                settings.For(x => x.Name).Trim(5));
            printedLine.Should().Contain("TEST");
        }

        [Test]
        public void GetStringRepresentation_ThrowsArgumentOutOfRangeException_OnWrongTrimValue()
        {
            var person = new Person();
            person.Name = "TEST";
            Action action = () => person.PrintToString(settings =>
                settings.For(x => x.Name).Trim(-5));
            action.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void GetStringRepresentation_ReturnsCorrectAnswer_OnCultureChange()
        {
            var value = 16325.62901;
            var person = new Person();
            person.Height = value;
            var valueWithCustomCulture =
                person.PrintToString(settings => settings.For(x => x.Height).WithCulture(CultureInfo.CreateSpecificCulture("eu-ES")));
            var valueWithCustomCulture2 = person.PrintToString(settings => settings.For(x => x.Height).WithCulture(CultureInfo.InvariantCulture));  
            valueWithCustomCulture.Should().Contain("16325,62901");
            valueWithCustomCulture2.Should().Contain("16325.62901");
        }
    }
}