using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterUnitTests
    {
        [Test]
        public void Exclude_ShouldExcludeIntPropertiesAndFields_WhenIntGeneric()
        {
            var person = PersonFactory.Get();
            var config = ObjectPrinter
                .For<Person>()
                .Exclude<int>();

            var serializedPerson = config.PrintToString(person);

            serializedPerson.Should().NotContain($"{nameof(Person.Money)}");
            serializedPerson.Should().NotContain($"{nameof(Person.Age)}");
        }

        [Test]
        public void Exclude_ShouldExcludeStringProperties_WhenStringGeneric()
        {
            var person = PersonFactory.Get();
            var config = ObjectPrinter
                .For<Person>()
                .Exclude<string>();

            var serializedPerson = config.PrintToString(person);

            serializedPerson.Should().NotContain($"{nameof(Person.Name)}");
        }

        [Test]
        public void When_Use_ShouldApplyFormattingOfType()
        {
            var person = PersonFactory.Get();
            var config = ObjectPrinter.For<Person>()
                .When<string>().Use(value => $"{value}{value}");

            var serializedPerson = config.PrintToString(person);

            serializedPerson.Should().Contain($"{nameof(Person.Name)} = {person.Name}{person.Name}");
        }

        [Test]
        public void When_Use_ShouldApplyCultureFormattingOfType()
        {
            var culture = new CultureInfo("en-GB");
            var person = PersonFactory.Get();
            var config = ObjectPrinter.For<Person>()
                .When<double>().Use(culture);

            var serializedPerson = config.PrintToString(person);

            serializedPerson.Should().Contain($"{nameof(Person.Height)} = {person.Height.ToString(culture)}");
        }

        [Test]
        public void When_Use_ShouldThrowException_WhenUseCultureFormattingOfType()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                ObjectPrinter.For<Person>()
                    .When<double>().Use((IFormatProvider)null);
            });
        }

        [Test]
        public void When_Use_ShouldApplyPropertyFormatting()
        {
            var person = PersonFactory.Get();
            var config = ObjectPrinter.For<Person>()
                .When(p => p.Age).Use(age => $"{age} years");

            var serializedPerson = config.PrintToString(person);

            serializedPerson.Should().Contain($"{nameof(Person.Age)} = {person.Age} years");
        }

        [Test]
        public void When_Use_ShouldApplyMemberFormatting()
        {
            var person = PersonFactory.Get();
            var config = ObjectPrinter.For<Person>()
                .When(p => p.Money).Use(money => $"{money} RUB");

            var serializedPerson = config.PrintToString(person);

            serializedPerson.Should().Contain($"{nameof(Person.Money)} = {person.Money} RUB");
        }

        [Test]
        public void When_UseSubstring_ShouldTakeSubstring()
        {
            var range = 1..2;
            var person = PersonFactory.Get();
            var config = ObjectPrinter.For<Person>()
                .When<string>().UseSubstring(range);

            var serializedPerson = config.PrintToString(person);

            serializedPerson.Should().Contain($"{nameof(Person.Name)} = {person.Name[range]}");
        }


        [Test]
        public void Exclude_ShouldExcludeMember()
        {
            var person = PersonFactory.Get();
            var config = ObjectPrinter.For<Person>()
                .Exclude(p => p.Country);

            var serializedPerson = config.PrintToString(person);

            serializedPerson.Should().NotContain($"{nameof(Person.Name)} = {person.Country}");
        }

        [Test]
        public void SetAllowCycleReference_ShouldFormatCycleReference_WhenAllow()
        {
            var person = PersonFactory.Get();
            var house = HouseFactory.Get();
            person.House = house;
            house.Owner = person;
            var config = ObjectPrinter.For<Person>()
                .SetAllowCycleReference(true);

            var serializedPerson = config.PrintToString(person);

            serializedPerson.Should().Contain($"{nameof(House.Owner)} = {{...}}");
        }

        [Test]
        public void SetAllowCycleReference_ShouldThrowException_WhenNotAllow()
        {
            var person = PersonFactory.Get();
            var house = HouseFactory.Get();
            person.House = house;
            house.Owner = person;
            var config = ObjectPrinter.For<Person>()
                .SetAllowCycleReference(false);

            Assert.Throws<InvalidOperationException>(() => config.PrintToString(person));
        }

        [Test]
        public void When_Use_ShouldApplyPropertyFormattingInsteadOfTypeFormatting()
        {
            var person = PersonFactory.Get();
            var config = ObjectPrinter.For<Person>()
                .When<int>().Use(n => $"-{n}")
                .When(p => p.Money).Use(money => $"{money}RUB");

            var serializedObject = config.PrintToString(person);

            serializedObject.Should()
                .Contain($"{nameof(Person.Age)} = -{person.Age}")
                .And
                .Contain($"{nameof(Person.Money)} = {person.Money}RUB");
        }

        [Test]
        public void When_Use_FormattingType_ShouldThrowException_WhenNullTransformer()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                ObjectPrinter.For<Person>()
                    .When<int>().Use(null);
            });
        }

        [Test]
        public void When_Use_FormattingProperty_ShouldThrowException_WhenNullTransformer()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                ObjectPrinter.For<Person>()
                    .When(p => p.Age).Use(null);
            });
        }

        [Test]
        public void When_SelectProperty_ShouldThrowException_WhenNullSelector()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                ObjectPrinter.For<Person>()
                    .When<string>(null);
            });
        }

        [Test]
        public void Exclude_ShouldThrowException_WhenNullSelector()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                ObjectPrinter.For<Person>()
                    .Exclude<string>(null);
            });
        }

        [Test]
        public void When_SelectProperty_ShouldThrowException_WhenIncorrectExpression()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ObjectPrinter.For<Person>()
                    .When(p => p.GetType());
            });
        }

        [Test]
        public void Exclude_SelectProperty_ShouldThrowException_WhenIncorrectExpression()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                ObjectPrinter.For<Person>()
                    .Exclude(p => p.GetType());
            });
        }
    }
}