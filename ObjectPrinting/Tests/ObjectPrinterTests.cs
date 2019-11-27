using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;


namespace ObjectPrinting.Tests
{
    [TestFixture]
    class ObjectPrinterTests
    {
        private readonly Person person = new Person {Name = "Alex", Age = 19, Height = 1.2};

        [Test]
        public void Excluding_ShouldExcludeType()
        {
            ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .PrintToString(person)
                .Should()
                .NotContain("Guid =");
        }

        [Test]
        public void Excluding_ShouldExcludeProperty()
        {
            ObjectPrinter.For<Person>()
                .Excluding(p => p.Age)
                .PrintToString(person)
                .Should()
                .NotContain("Age");
        }

        [Test]
        public void Using_ShouldSetFormattingMethodForType()
        {
            ObjectPrinter.For<Person>()
                .Printing<int>()
                .Using(i => $"foo {i} bar")
                .PrintToString(person)
                .Should()
                .Contain("foo 19 bar");
        }

        [Test]
        public void Using_ShouldSetFormattingMethodForProperty()
        {
            ObjectPrinter.For<Person>()
                .Printing(p => p.Age)
                .Using(i => $"foo {i} bar")
                .PrintToString(person)
                .Should()
                .Contain("foo 19 bar");
        }

        [Test]
        public void TrimmedToLength_ShouldTrimProperty_WhenMaxLengthLessStringLength()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing(p => p.Name)
                .TrimmedToLength(2)
                .PrintToString(person);

            result.Should()
                .NotContain("Alex");

            result.Should().Contain("Al");
        }

        [Test]
        public void TrimmedToLength_ShouldNotTrimProperty_WhenMaxLengthGreaterOrEqualStringLength()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing(p => p.Name)
                .TrimmedToLength(5)
                .PrintToString(person);

            result.Should()
                .Contain("Alex");
        }

        [Test]
        public void Using_ShouldUseGivenCulture_WhenSelectedNumber()
        {
            ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(new CultureInfo("en-us"))
                .PrintToString(person)
                .Should()
                .Contain("1.2");
        }

        [Test]
        public void PrintToString_ShouldFormatCorrectly_WhenFinalType()
        {
            42.PrintToString()
                .Should()
                .BeEquivalentTo($"42{Environment.NewLine}");
        }

        [Test]
        public void PrintToString_ShouldFormatCorrectly_WhenComplexEntity()
        {
            person.PrintToString()
                .Should()
                .BeEquivalentTo(
                    $"Person{Environment.NewLine}\t" +
                    $"Id = Guid{Environment.NewLine}\t" +
                    $"Name = Alex{Environment.NewLine}\t" +
                    $"Height = 1,2{Environment.NewLine}\t" +
                    $"Age = 19{Environment.NewLine}");
        }

        [Test]
        public void PrintToString_ShouldFormatCorrectly_WhenIEnumerable()
        {
            new[] {1, 2, 3, 4}.PrintToString()
                .Should()
                .BeEquivalentTo("1, 2, 3, 4");
        }

        [Test]
        public void PrintToString_ShouldFormatCorrectly_WhenCycle()
        {
            var cyclicObject = new CyclicObject();
            cyclicObject.Other = cyclicObject;
            cyclicObject.PrintToString()
                .Should().BeEquivalentTo(
                    $"CyclicObject{Environment.NewLine}\t" +
                    $"Id = 0{Environment.NewLine}\t" +
                    $"Other = !cycle!{Environment.NewLine}");
        }
    }
}