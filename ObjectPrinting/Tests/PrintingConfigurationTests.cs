using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class PrintingConfigurationTests
    {
        [Test]
        public void PrintingType_UsesCustomFormatterForAllMembersOfType()
        {
            var person = new Person
            {
                Age = 19,
                Score = 42
            };

            var result = ObjectPrinter.For<Person>()
                .Printing<int>()
                .Using(value => $"#{value}")
                .PrintToString(person);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("Age = #19"));
                Assert.That(result, Does.Contain("Score = #42"));
            });
        }

        [Test]
        public void PrintingType_UsesSpecifiedCulture()
        {
            var person = new Person
            {
                Height = 1.5
            };

            var result = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(CultureInfo.GetCultureInfo("fr-FR"))
                .PrintToString(person);

            Assert.That(result, Does.Contain("Height = 1,5"));
        }

        [Test]
        public void PrintingMember_UsesCustomFormatter()
        {
            var person = new Person
            {
                Age = 19
            };

            var result = ObjectPrinter.For<Person>()
                .Printing(person => person.Age)
                .Using(age => $"Возраст: {age}")
                .PrintToString(person);

            Assert.That(result, Does.Contain("Age = Возраст: 19"));
        }

        [Test]
        public void PrintingMember_UsesSpecifiedCulture()
        {
            var person = new Person
            {
                Height = 1.5
            };

            var result = ObjectPrinter.For<Person>()
                .Printing(person => person.Height)
                .Using(CultureInfo.GetCultureInfo("fr-FR"))
                .PrintToString(person);

            Assert.That(result, Does.Contain("Height = 1,5"));
        }

        [Test]
        public void PrintingStringMember_TrimsValue()
        {
            var person = new Person
            {
                Name = "Alexander"
            };

            var result = ObjectPrinter.For<Person>()
                .Printing(person => person.Name)
                .TrimmedToLength(4)
                .PrintToString(person);

            Assert.That(result, Does.Contain("Name = Alex"));
            Assert.That(result, Does.Not.Contain("Alexander"));
        }

        [Test]
        public void ExcludingMember_DoesNotPrintSelectedProperty()
        {
            var person = new Person
            {
                Age = 19
            };

            var result = ObjectPrinter.For<Person>()
                .Excluding(person => person.Age)
                .PrintToString(person);

            Assert.That(result, Does.Not.Contain("Age = "));
            Assert.That(result, Does.Contain("Name = "));
        }

        [Test]
        public void ExcludingMember_DoesNotPrintSelectedField()
        {
            var person = new Person
            {
                Score = 42
            };

            var result = ObjectPrinter.For<Person>()
                .Excluding(person => person.Score)
                .PrintToString(person);

            Assert.That(result, Does.Not.Contain("Score = "));
        }

        [Test]
        public void MemberFormatter_TakesPrecedenceOverTypeFormatter()
        {
            var person = new Person
            {
                Age = 19,
                Score = 42
            };

            var result = ObjectPrinter.For<Person>()
                .Printing<int>()
                .Using(value => $"Type: {value}")
                .Printing(person => person.Age)
                .Using(value => $"Member: {value}")
                .PrintToString(person);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("Age = Member: 19"));
                Assert.That(result, Does.Contain("Score = Type: 42"));
            });
        }

        [Test]
        public void TrimmedToLength_RejectsNegativeLength()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ObjectPrinter.For<Person>()
                    .Printing(person => person.Name)
                    .TrimmedToLength(-1));
        }

        [Test]
        public void PrintingMember_RejectsNestedMemberExpression()
        {
            Assert.Throws<ArgumentException>(() =>
                ObjectPrinter.For<Person>()
                    .Printing(person => person.Name.Length));
        }
        
        [Test]
        public void Printing_ThrowsWhenExpressionDoesNotSelectMember()
        {
            Assert.Throws<ArgumentException>(() =>
                ObjectPrinter.For<Person>()
                    .Printing(person => 42));
        }
    }
}