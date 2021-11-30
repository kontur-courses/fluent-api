using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterTests
    {
        [Test]
        public void Should_DoNothing_When_NoSettings()
        {
            var person = PersonFactory.CreateDefaultPerson();
            var expected = new StringBuilder()
                .AppendLine($"{nameof(Person)}")
                .AppendLine($"\t{nameof(person.LastName)} = {person.LastName}")
                .AppendLine($"\t{nameof(person.Id)} = {person.Id}")
                .AppendLine($"\t{nameof(person.FirstName)} = {person.FirstName}")
                .AppendLine($"\t{nameof(person.Height)} = {person.Height}")
                .AppendLine($"\t{nameof(person.Age)} = {person.Age}")
                .AppendLine($"\t{nameof(person.Parent)} = null")
                .ToString();

            var printedPerson = ObjectPrinter.For<Person>()
                .PrintToString(person);

            printedPerson.Should().Be(expected);
        }

        [Test]
        public void Should_ThrowException_WhenSelectorIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => 
                ObjectPrinter.For<Person>()
                    .Use(x => x.Age).With(null));
        }

        [Test]
        public void Should_Exclude_Properties()
        {
            var person = PersonFactory.CreateDefaultPerson();

            var printedPerson = ObjectPrinter.For<Person>()
                .Exclude(x => x.FirstName)
                .PrintToString(person);

            printedPerson.Should().NotContain(nameof(person.FirstName));
        }

        [Test]
        public void Should_Exclude_Fields()
        {
            var person = PersonFactory.CreateDefaultPerson();

            var printedPerson = ObjectPrinter.For<Person>()
                .Exclude(x => x.LastName)
                .PrintToString(person);

            printedPerson.Should().NotContain(nameof(person.LastName));
        }

        [Test]
        public void Should_Exclude_Types()
        {
            var person = PersonFactory.CreateDefaultPerson();

            var printedPerson = ObjectPrinter.For<Person>()
                .Exclude<Guid>()
                .PrintToString(person);

            printedPerson.Should().NotContain(nameof(person.Id));
        }

        [Test]
        public void Should_ThrowException_When_NotMemberSelectorProvided()
        {
            var person = PersonFactory.CreateDefaultPerson();

            void PrintedPersonAction() =>
                ObjectPrinter.For<Person>()
                    .Exclude(x => "f")
                    .PrintToString(person);

            Assert.Throws<ArgumentException>(PrintedPersonAction);
        }

        [Test]
        public void Should_Support_CustomTypeSerialization()
        {
            var person = PersonFactory.CreateDefaultPerson();

            var printedPerson = ObjectPrinter.For<Person>()
                .Use<string>().With(x => $"\"{x}\"")
                .PrintToString(person);

            printedPerson.Should().Contain($"{nameof(person.LastName)} = \"{person.LastName}\"")
                .And.Contain($"{nameof(person.FirstName)} = \"{person.FirstName}\"");
        }

        [Test]
        public void Should_Support_CustomMemberSerialization()
        {
            var person = PersonFactory.CreateDefaultPerson();

            var printedPerson = ObjectPrinter.For<Person>()
                .Use(x => x.FirstName).With(x => $"cool {x}")
                .PrintToString(person);

            printedPerson.Should().Contain($"{nameof(person.FirstName)} = cool {person.FirstName}");
        }

        [Test]
        public void Should_Support_CustomTypeCulture()
        {
            var culture = new CultureInfo("en-GB");
            var person = PersonFactory.CreateDefaultPerson();

            var printedPerson = ObjectPrinter.For<Person>()
                .Use<double>().With(culture)
                .PrintToString(person);

            printedPerson.Should().Contain($"{nameof(person.Height)} = {person.Height.ToString(culture)}");
        }

        [Test]
        public void Should_Support_CustomMemberCulture()
        {
            var culture = new CultureInfo("en-GB");
            var person = PersonFactory.CreateDefaultPerson();

            var printedPerson = ObjectPrinter.For<Person>()
                .Use(x => x.Age).With(culture)
                .PrintToString(person);

            printedPerson.Should().Contain($"{nameof(person.Age)} = {person.Age.ToString(culture)}");
        }

        [Test]
        public void Should_Support_StringsTrimming()
        {
            var person = PersonFactory.CreateDefaultPerson();

            var printedPerson = ObjectPrinter.For<Person>()
                .Use(x => x.FirstName).WithTrimming(3)
                .PrintToString(person);

            printedPerson.Should()
                .Contain($"{nameof(person.FirstName)} = {person.FirstName.Substring(0, 3)}");
        }

        [Test]
        public void Should_DetectCyclicReferences_AndDontThrowByDefault()
        {
            var person = PersonFactory.CreatePersonWithCycleReference();

            var printedPerson = ObjectPrinter.For<Person>()
                .UseCycleReference(true)
                .PrintToString(person);

            printedPerson.Should().Contain($"{nameof(person.Parent)} = ![Cyclic reference]!");
        }

        [Test]
        public void Should_DetectCyclicReferences_AndControlIt()
        {
            var person = PersonFactory.CreatePersonWithCycleReference();

            var personPrinter = ObjectPrinter.For<Person>()
                .UseCycleReference();

            Assert.Throws<Exception>(() => personPrinter.PrintToString(person));
        }

        [Test]
        public void Should_PrintArray()
        {
            var cities = new[] {"Moscow", "Rio", "Zurich"};
            var expected = new StringBuilder()
                .AppendLine("[")
                .AppendLine("\tMoscow")
                .AppendLine("\tRio")
                .AppendLine("\tZurich")
                .AppendLine("]")
                .ToString();
            
            var printedArray = ObjectPrinter.For<string[]>()
                .PrintToString(cities);

            printedArray.Should().Be(expected);
        }

        [Test]
        public void Should_PrintList()
        {
            var cities = new List<string> {"Moscow", "Rio", "Zurich"};
            var expected = new StringBuilder()
                .AppendLine("[")
                .AppendLine("\tMoscow")
                .AppendLine("\tRio")
                .AppendLine("\tZurich")
                .AppendLine("]")
                .ToString();
            
            var printedArray = ObjectPrinter.For<List<string>>()
                .PrintToString(cities);

            printedArray.Should().Be(expected);
        }

        [Test]
        public void Should_PrintDictionary()
        {
            var currencies = new Dictionary<string, int>
            {
                {"RUB", 70},
                {"USD", 100}
            };
            var expected = new StringBuilder()
                .AppendLine("[")
                .AppendLine("\tKeyValuePair`2")
                .AppendLine("\t\tKey = RUB")
                .AppendLine("\t\tValue = 70")
                .AppendLine("\tKeyValuePair`2")
                .AppendLine("\t\tKey = USD")
                .AppendLine("\t\tValue = 100")
                .AppendLine("]")
                .ToString();
            
            var printedArray = ObjectPrinter.For<Dictionary<string, int>>()
                .PrintToString(currencies);

            printedArray.Should().Be(expected);
        }
    }
}