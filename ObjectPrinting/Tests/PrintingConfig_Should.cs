using System;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class PrintingConfig
    {
        [Test]
        public void PrintToString_Null_ReturnsNull() => ObjectPrinter.For<string>().PrintToString(null)
            .Should()
            .Be("null" + Environment.NewLine);

        [Test]
        public void PrintToString_PersonWithoutExcludingAnything_ContainsAllPublicProperties()
        {
            var personProperties = typeof(Person).GetProperties().Select(p => p.Name).ToList();
            new Person().PrintToString().Should().ContainAll(personProperties);
        }

        [Test]
        public void PrintToString_PersonExcludingGuidValues_NotContainsGuid() =>
            PersonDoesntContain(nameof(Guid), c => c.Excluding<Guid>());

        [Test]
        public void PrintToString_PersonExcludingProperty_NotContainsProperty() =>
            PersonDoesntContain(nameof(Person.Spouse), c => c.Excluding(p => p.Spouse));

        [Test]
        public void PrintToString_PersonExcludingField_NotContainsProperty() =>
            PersonDoesntContain(nameof(Person.Id), c => c.Excluding(p => p.Id));

        [Test]
        public void PrintToString_PersonHasAlternativePrintForDouble_ContainsAlternativeStringInsteadDouble() =>
            PersonContains("this is double",
                pc => pc.Printing<double>().Using(d => "this is double"));

        [Test]
        public void PrintToString_PersonHasAlternativePrintForHeightProperty_ContainsAlternativeStringForHeight() =>
            PersonContains("this is height",
                pc => pc.Printing(person => person.Height).Using(d => "this is height"));

        [Test]
        public void PrintToString_PersonHasAlternativePrintForIdField_ContainsAlternativeStringForHeight() =>
            PersonContains("this is personal ID",
                pc => pc.Printing(person => person.Id).Using(d => "this is personal ID"));

        [Test]
        public void PrintToString_PersonHasInvariantCultureForDateTime_ContainsInvariantDateTime() =>
            PersonContains("01/01/0001 00:00:00",
                pc => pc.Printing<DateTime>().Using(CultureInfo.InvariantCulture));

        [Test]
        public void PrintToString_PersonHasCurrentCultureForDouble_ContainsCorrespondingDouble()
        {
            var person = new Person {Height = 1.78};
            person
                .PrintToString(p => p.Printing<double>().Using(CultureInfo.CurrentCulture))
                .Should()
                .Contain("Height = 1,78");
        }

        [Test]
        public void PrintToString_TrimmedPersonStrings_ContainsOnlyTrimmedStrings()
        {
            var person = new Person {Name = "Humpty", Surname = "Dumpty"};
            person
                .PrintToString(c => c.Printing<string>().TrimmedToLength(3))
                .Should()
                .NotContainAll("Humpty", "Dumpty")
                .And
                .ContainAll("Hum", "Dum");
        }

        [Test]
        public void PrintToString_DifferentConfigs_ReturnsDifferentResults()
        {
            var config = ObjectPrinter.For<Person>();
            var first = config.Excluding(p => p.Id);
            var second = config.Excluding<int>().Printing<string>().Using(s => "bar");
            first = first.Printing<string>().Using(s => "foo");


            var firstResult = first.PrintToString(new Person());
            var secondResult = second.PrintToString(new Person());

            firstResult.Should().Contain("Age");
            secondResult.Should().Contain("Id");

            firstResult.Should().NotContain("bar");
            secondResult.Should().NotContain("foo");
        }

        [Test]
        public void PrintToString_IEnumerable_ReturnsCorrectly() =>
            Enumerable.Range(0, 10)
                .PrintToString()
                .Should()
                .ContainAll(Enumerable.Range(0, 10).Select(i => i.ToString()));

        [Test]
        public void PrintToString_Array_ReturnsCorrectly()
        {
            var array = Enumerable.Range(0, 10).Select(i => i.ToString()).ToArray();
            array.PrintToString().Should().ContainAll(array);
        }

        [Test]
        public void PrintToString_List_ReturnsCorrectly()
        {
            var list = Enumerable.Range(0, 10).Select(i => i.ToString()).ToList();
            list.PrintToString().Should().ContainAll(list);
        }

        [Test]
        public void PrintToString_Dictionary_ReturnsCorrectly()
        {
            var dictionary = Enumerable.Range(0, 10)
                .Select(_ => new Person
                {
                    Id = Guid.NewGuid(), BirthDay = DateTime.Now, Height = new Random().Next(0, 100)
                })
                .ToDictionary(p => p.Id, p => p);
            dictionary.PrintToString().Should().ContainAll(dictionary.ToList().PrintToString().Substring(7));
        }

        private void PersonDoesntContain(string expected, Func<PrintingConfig<Person>, PrintingConfig<Person>> conf) =>
            new Person()
                .PrintToString(conf)
                .Should()
                .NotContain(expected);

        private void PersonContains(string expected, Func<PrintingConfig<Person>, PrintingConfig<Person>> conf) =>
            new Person()
                .PrintToString(conf)
                .Should()
                .Contain(expected);
    }
}