using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        private Person person;

        [SetUp]
        public void SetUp()
        {
            person = new Person
            {
                Name = "Alex",
                Surname = "VeryLongSurname",
                Age = 19,
                Height = 185.5,
                Weight = 63.5
            };
        }


        [Test]
        public void PrintToString_ShouldContainAllProperties_WithoutConfiguration()
        {
            var expectedProperties = person.GetType().GetProperties().Select(property => property.Name);
            var result = ObjectPrinter.For<Person>().PrintToString(person);
            result.Should().ContainAll(expectedProperties);
        }

        [Test]
        public void PrintToString_ShouldContainAllPropertyValues_WithoutConfiguration()
        {
            var expectedProperties = person.GetType().GetProperties().Select(property => property.GetValue(person)?.ToString());
            var result = ObjectPrinter.For<Person>().PrintToString(person);
            result.Should().ContainAll(expectedProperties);
        }

        [Test]
        public void PrintToString_ShouldNotContainPropertyByType_AfterExclude()
        {
            var result = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .PrintToString(person);
            result.Should().NotContain(nameof(Guid));
        }

        [Test]
        public void PrintToString_ShouldUseSpecialFuncForType_AfterUsing()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing<int>().Using(number => "check")
                .PrintToString(person);
            result.Should().Contain($"{nameof(Person.Age)} = check");
        }

        [Test]
        public void PrintToString_ShouldUseCultureForNumbers_AfterUsing()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .PrintToString(person);
            result.Should().Contain($"{nameof(Person.Height)} = {person.Height.ToString(CultureInfo.InvariantCulture)}");
        }

        [Test]
        public void PrintToString_ShouldUseSpecialFuncForProperty_AfterUsing()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing(p => p.Weight).Using(weight => "check")
                .PrintToString(person);
            result.Should().Contain($"{nameof(Person.Weight)} = check");
        }

        [Test]
        public void PrintToString_ShouldTrimStrings_AfterTrimmedToLength()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing(p => p.Surname).TrimmedToLength(10)
                .PrintToString(person);
            result.Should().Contain($"{nameof(Person.Surname)} = {person.Surname.Substring(0, 10)}");
        }

        [Test]
        public void PrintToString_ShouldNotThrowExceptionOnShortStrings_AfterTrimmedToLength()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(100)
                .PrintToString(person);
            result.Should().Contain($"{nameof(Person.Name)} = {person.Name}");
        }

        [Test]
        public void PrintToString_ShouldNotContainExcludedProperty_AfterExclude()
        {
            var result = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age)
                .PrintToString(person);
            result.Should().NotContain(nameof(Person.Age));
        }

        [Test]
        public void PrintToStringAsExtensionMethod_ShouldContainAllProperties_WithoutConfiguration()
        {
            var expectedProperties = person.GetType().GetProperties().Select(property => property.Name);
            var result = person.PrintToString();
            result.Should().ContainAll(expectedProperties);
        }

        [Test]
        public void PrintToStringAsExtensionMethod_ShouldContainAllPropertyValues_WithoutConfiguration()
        {
            var expectedProperties = person.GetType().GetProperties().Select(property => property.GetValue(person)?.ToString());
            var result = person.PrintToString();
            result.Should().ContainAll(expectedProperties);
        }

        [Test]
        public void PrintToStringAsExtensionMethod_ShouldApplyConfiguration_WithConfiguration()
        {
            var result = person.PrintToString(config => config.Excluding(p => p.Age));
            result.Should().NotContain(nameof(Person.Age));
        }

        [Test]
        public void PrintToString_ShouldContainAllItems_InvokedWithCollection()
        {
            var collection = new[] {1, 2, 3};
            var result = ObjectPrinter.For<int[]>().PrintToString(collection);
            result.Should().ContainAll(collection.Select(item => item.ToString()));
        }

        [Test]
        public void PrintToString_ShouldApplyConfiguration_InvokedWithCollection()
        {
            var collection = new[] { 1.1, 2.2, 3.3 };
            var result = ObjectPrinter.For<double[]>()
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .PrintToString(collection);
            result.Should().ContainAll(collection.Select(item => item.ToString(CultureInfo.InvariantCulture)));
        }

        [Test]
        public void PrintToString_ShouldNotThrowStackOverflowException_OnInfiniteRecursion()
        {
            person.Parent = person;
            var result = ObjectPrinter.For<Person>().PrintToString(person);
            result.Should().Contain($"{nameof(Person.Parent)} = [Infinite Recursion]");
        }

        [Test]
        public void PrintToString_ShouldNotThrowStackOverflowException_OnInfiniteCollection()
        {
            person.Parent = person;
            var result = ObjectPrinter.For<IEnumerable<int>>().PrintToString(Numbers.GetNumbers());
            result.Should().Contain("...");
        }
    }
}