using System;
using NUnit.Framework;
using FluentAssertions;

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
            var config = ObjectPrinter
                .For<Person>()
                .When<string>()
                .Use(value => $"{value}{value}{Environment.NewLine}");

            var serializedPerson = config.PrintToString(person);
            
            serializedPerson.Should().Contain($"{nameof(Person.Name)} = {person.Name}{person.Name}");
        }
    }
}