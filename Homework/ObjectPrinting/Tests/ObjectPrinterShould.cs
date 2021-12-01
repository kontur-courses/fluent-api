using FluentAssertions;
using NUnit.Framework;
using System;
using System.Globalization;
using static System.Environment;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterShould
    {
        private Person _vasya = new Person
        {
            Id = Guid.NewGuid(),
            Name = "Vasya",
            Height = 180,
            Age = 26,
            weight = 200,
            secondName = "Minin"
        };

        [Test]
        public void ExcludeChosenTypes()
        {
            var expectedSerialization = "Person" + NewLine +
                $"\tId = {_vasya.Id}" + NewLine +
                $"\tAge = {_vasya.Age}" + NewLine +
                $"\tNonCulturable = null" + NewLine +
                $"\tweight = {_vasya.weight}" + NewLine +
                $"\tsecondName = {_vasya.secondName}" + NewLine;

            var printer = ObjectPrinter
                .For<Person>()
                .Excluding<string>()
                .Excluding<double>();
            string serializedPerson = printer.PrintToString(_vasya);

            serializedPerson.Should().Be(expectedSerialization);
        }

        [Test]
        public void ApplyTypeAlternativeSerializationCorrect()
        {
            Func<int, string> intConfig = x => x.ToString() + " is not my real age!" + NewLine;
            Func<Guid, string> guidConfig = x => "guid imitation" + NewLine;
            var expectedSerialization = "Person" + NewLine +
                $"\tguid imitation" + NewLine +
                $"\tName = {_vasya.Name}" + NewLine +
                $"\tHeight = {_vasya.Height}" + NewLine +
                $"\t{_vasya.Age} is not my real age!" + NewLine +
                $"\tNonCulturable = null" + NewLine +
                $"\tweight = {_vasya.weight}" + NewLine +
                $"\tsecondName = {_vasya.secondName}" + NewLine;

            var printer = ObjectPrinter
                 .For<Person>()
                 .Printing<int>().Using(intConfig)
                 .Printing<Guid>().Using(guidConfig);

            var serializatedPerson = printer.PrintToString(_vasya);
            serializatedPerson.Should().BeEquivalentTo(expectedSerialization);
            Console.WriteLine(serializatedPerson);
        }

        [Test]
        public void ThrowExceptionWhenNonCultureTypeWasCultured()
        {
            Func<PrintingConfig<Person>> setCultureToNonCulturable = () => ObjectPrinter
                .For<Person>()
                .Printing<NonCulturable>().Using(new CultureInfo("en-US"));

            setCultureToNonCulturable.Should().Throw<Exception>();
        }

        [Test]
        public void ApplyTypeCultureCorrect()
        {
            var culture = new CultureInfo("en-US");
            var doubleWrapper = new DoubleWrapper();
            var culturedDouble = doubleWrapper.WrappedNumber.ToString(culture);
            var expectedSerialization = "DoubleWrapper" + NewLine +
                $"\twrappedNumber = {culturedDouble}" + NewLine;

            var doublePrinter = ObjectPrinter
                .For<DoubleWrapper>()
                .Printing<double>().Using(culture);
            var serializationResult = doublePrinter.PrintToString(doubleWrapper);

            serializationResult.Should().BeEquivalentTo(expectedSerialization);
        }
    }
}