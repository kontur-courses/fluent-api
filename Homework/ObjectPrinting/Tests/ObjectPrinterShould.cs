using System;
using NUnit.Framework;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterShould
    {
        [Test]
        public void ExcludeChosenTypes()
        {
            var person = new Person {Id = Guid.NewGuid(), Name = "Vasya", Height = 180,
                Age = 26, weight = 200, secondName = "Minin" };
            var expectedSerialization = "Person" + Environment.NewLine + 
                $"\tId = {person.Id}" + Environment.NewLine + 
                $"\tHeight = {person.Height}" + Environment.NewLine + 
                $"\tweight = {person.weight}" + Environment.NewLine;

            var printer = ObjectPrinter
                .For<Person>()
                .ExcludePropertiesTypes(typeof(string), typeof(int))
                .ExcludeFieldsTypes(typeof(string), typeof(int));
            string serializedPerson = printer.PrintToString(person);

            serializedPerson.Should().Be(expectedSerialization);
        }
    }
}