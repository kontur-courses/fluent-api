using FluentAssertions;
using NUnit.Framework;
using System;
using static System.Environment;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterShould
    {
        [Test]
        public void ExcludeChosenTypes()
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                Name = "Vasya",
                Height = 180,
                Age = 26,
                weight = 200,
                secondName = "Minin"
            };
            var expectedSerialization = "Person" + NewLine +
                $"\tId = {person.Id}" + NewLine +
                $"\tAge = {person.Age}" + NewLine +
                $"\tNonCulturable = null" + NewLine +
                $"\tweight = {person.weight}" + NewLine +
                $"\tsecondName = {person.secondName}" + NewLine;

            var printer = ObjectPrinter
                .For<Person>()
                .Excluding<string>()
                .Excluding<double>();
            string serializedPerson = printer.PrintToString(person);

            serializedPerson.Should().Be(expectedSerialization);
        }

        [Test]
        public void ApplyTypeAlternativeSerializationCorrect()
        {
            Func<int, string> intConfig = x => x.ToString() + " is not my real age!" + NewLine;
            Func<Guid, string> guidConfig = x => "guid imitation" + NewLine;
            var person = new Person
            {
                Id = Guid.NewGuid(),
                Name = "Vasya",
                Height = 180,
                Age = 26,
                weight = 200,
                secondName = "Minin"
            };
            var expectedSerialization = "Person" + NewLine +
                $"\tguid imitation" + NewLine +
                $"\tName = {person.Name}" + NewLine +
                $"\tHeight = {person.Height}" + NewLine +
                $"\t{person.Age} is not my real age!" + NewLine +
                $"\tNonCulturable = null" + NewLine +
                $"\tweight = {person.weight}" + NewLine +
                $"\tsecondName = {person.secondName}" + NewLine;

            var printer = ObjectPrinter
                 .For<Person>()
                 .Printing<int>().Using(intConfig)
                 .Printing<Guid>().Using(guidConfig);

            var serializatedPerson = printer.PrintToString(person);
            serializatedPerson.Should().BeEquivalentTo(expectedSerialization);
            Console.WriteLine(serializatedPerson);
        }
    }
}