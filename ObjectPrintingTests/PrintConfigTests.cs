using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests
{
    public class PrintConfigTests
    {
        [Test]
        public void PrintToString_AcceptanceTest()
        {
            var person = new Person { Name = "Alex", Age = 255 };
            var culture = CultureInfo.InvariantCulture;
            
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(culture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(2)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Friend);

            var actual = printer.PrintToString(person);

            var expected = new StringBuilder()
                .AppendLine($"{nameof(Person)}")
                .AppendLine($"\t{nameof(person.Name)} = {person.Name[..2]}")
                .AppendLine($"\t{nameof(person.Height)} = {person.Height.ToString(culture)}")
                .AppendLine($"\t{nameof(person.Age)} = {person.Age:X}")
                .ToString();

            actual.Should().Be(expected);
        }
        
        
        [Test]
        public void PrintToString_ShouldPrintWithoutProperty_WhenItHasExcludedType()
        {
            var person = GetDefaultPerson();

            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>();
            var actual = printer.PrintToString(person);
            
            actual.Should().NotContain($"{nameof(person.Age)}");
        }

        [Test]
        public void PrintToString_ShouldPrintWithAlternativeWayOfSerialization_WhenItProvided()
        {
            var person = GetDefaultPerson();

            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(i => $"_{i}_");
            var actual = printer.PrintToString(person);
            
            actual.Should().Contain($"{nameof(person.Age)} = _{person.Age}_");
        }
        
        [Test]
        public void PrintToString_ShouldPrintWithAlternativeWayOfCulture_WhenItProvided()
        {
            var person = GetDefaultPerson();
            var culture = CultureInfo.InvariantCulture;

            var printer = ObjectPrinter.For<Person>()
                .Printing<double>().Using(culture);
            var actual = printer.PrintToString(person);
            
            actual.Should().Contain($"{nameof(person.Height)} = {person.Height.ToString(culture)}");
        }
        
        [Test]
        public void PrintToString_ShouldPrintPropertyWithAlternativeWay_WhenItProvided()
        {
            var person = GetDefaultPerson();

            var printer = ObjectPrinter.For<Person>()
                .Printing(p=> p.Age).Using(x => $"!{x}!");
            var actual = printer.PrintToString(person);

            actual.Should().Contain($"{nameof(person.Age)} = !{person.Age}!");
        }
        
        [Test]
        public void PrintToString_ShouldPrintTrimmedString_WhenItProvided()
        {
            var person = GetDefaultPerson();

            var printer = ObjectPrinter.For<Person>()
                .Printing<string>().TrimmedToLength(3);
            var actual = printer.PrintToString(person);
            
            actual.Should().Contain($"{nameof(person.Name)} = Iva");
        }
        
        [Test]
        public void PrintToString_ShouldPrintWithoutProperty_WhenItExcluded()
        {
            var person = GetDefaultPerson();

            var printer = ObjectPrinter.For<Person>()
                .Excluding(p => p.Name);
            var actual = printer.PrintToString(person);

            actual.Should().NotContain($"{nameof(person.Name)}");
        }
        
        [Test]
        public void PrintToString_ShouldPrintCollections_WhenCollectionProvided()
        {
            var personList = GetPersonList();

            var printer = ObjectPrinter.For<List<Person>>()
                .Excluding<int>();
            var actual = printer.PrintToString(personList);

            actual.Should().ContainAll($"{nameof(Person.Name)} = ", "Ivan", "Danil");
        }
        
        [Test]
        public void PrintToString_ShouldPrintCycle_WhenCycleReference()
        {
            var person = GetCycleReferencePerson();

            var printer = ObjectPrinter.For<Person>()
                .AllowCyclingReference();
            var actual = printer.PrintToString(person);

            actual.Should().Contain($"{nameof(person.Friend)} = Cycle");
        }

        [Test]
        public void PrintToString_ShouldPrintWithNestingLevel_WhenItProvided()
        {
            var person = GetNestedPerson();

            var printer = ObjectPrinter.For<Person>()
                .SetDepthOfSerialize(2);
            var actual = printer.PrintToString(person);
            
            actual.Should()
                .ContainAll($"{nameof(Person.Name)} = ", "Ivan", "Danil").And
                .NotContain("Maksim");
        }
        
        
        [Test]
        public void PrintToString_ShouldPrintDictionary()
        {
            var persons = GetPersonDictionary();

            var printer = ObjectPrinter.For<Dictionary<int, Person>>();
            var actual = printer.PrintToString(persons);

            actual.Should().ContainAll("Key: 1", "Value: Person", "Key: 2");
        }
        
        [Test]
        public void PrintToString_ShouldThrowException_WhenCycleReferenceDoesntSet()
        {
            var person = GetCycleReferencePerson();

            var printer = ObjectPrinter.For<Person>();

            FluentActions.Invoking(
                    () => printer.PrintToString(person))
                .Should().Throw<Exception>()
                .WithMessage("Unexpected cycle reference");
        }
        
        [Test]
        public void PrintToString_ShouldThrowException_WhenTrimmedANegativeMaxLenght()
        {
            FluentActions.Invoking(
                    () => ObjectPrinter.For<Person>()
                        .Printing<string>().TrimmedToLength(-1))
                .Should().Throw<ArgumentException>();
        }
        
        [Test]
        public void PrintToString_ShouldThrowException_WhenIncorrectExpression()
        {
            FluentActions.Invoking(
                    () => ObjectPrinter.For<Person>()
                        .Excluding(p => true))
                .Should().Throw<ArgumentException>();
        }
        
        [Test]
        public void PrintToString_ShouldThrowException_WhenNullExpression()
        {
            FluentActions.Invoking(
                    () => ObjectPrinter.For<Person>()
                        .Excluding<int>(null))
                .Should().Throw<ArgumentException>();
        }
        
        
        private Person GetDefaultPerson() => new() { Name = "Ivan", Age = 20, Height = 194 };

        private Person GetCycleReferencePerson()
        {
            var person = GetDefaultPerson();
            person.Friend = person;
            return person;
        }
        
        private List<Person> GetPersonList() => new()
        {
            new Person { Name = "Ivan", Age = 20 },
            new Person { Name = "Danil", Age = 20 },
            new Person { Name = "Maksim", Age = 20}
        };

        private Person GetNestedPerson()
        {
            var person1 = new Person { Name = "Ivan" };
            var person2 = new Person { Name = "Danil" };
            var person3 = new Person { Name = "Maksim" };
            person2.Friend = person3;
            person1.Friend = person2;
            return person1;
        }
        
        private Dictionary<int, Person> GetPersonDictionary() => new()
            { { 1, new Person { Name = "Ivan" } }, { 2, new Person { Name = "Danil" } } };
    }
}