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
        public void Printing_UsesCustomFormatter()
        {
            var person = new Person
            {
                Age = 19
            };

            var result = ObjectPrinter.For<Person>()
                .Printing(p => p.Age)
                .Using(age => $"Возраст: {age}")
                .PrintToString(person);

            Assert.That(result, Does.Contain("Age = Возраст: 19"));
        }

        [Test]
        public void Printing_UsesSpecifiedCultureForMember()
        {
            var person = new Person
            {
                Height = 1.5
            };

            var result = ObjectPrinter.For<Person>()
                .Printing(p => p.Height)
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
                .Printing(p => p.Name)
                .TrimmedToLength(4)
                .PrintToString(person);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("Name = Alex"));
                Assert.That(result, Does.Not.Contain("Alexander"));
            });
        }

        [Test]
        public void PrintingStringMember_TrimsValueToZeroLength()
        {
            var person = new Person
            {
                Name = "Alexander"
            };

            var result = ObjectPrinter.For<Person>()
                .Printing(p => p.Name)
                .TrimmedToLength(0)
                .PrintToString(person);

            var expected =
                "Person" + PrintingTestFormat.NewLine +
                "\tId = " + System.Guid.Empty + PrintingTestFormat.NewLine +
                "\tName = " + PrintingTestFormat.NewLine +
                "\tHeight = 0" + PrintingTestFormat.NewLine +
                "\tAge = 0" + PrintingTestFormat.NewLine +
                "\tExternalId = " + System.Guid.Empty + PrintingTestFormat.NewLine +
                "\tScore = 42" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ExcludingMember_DoesNotPrintSelectedProperty()
        {
            var person = new Person
            {
                Age = 19
            };

            var result = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age)
                .PrintToString(person);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Not.Contain("Age = "));
                Assert.That(result, Does.Contain("Name = "));
            });
        }

        [Test]
        public void ExcludingMember_DoesNotPrintSelectedField()
        {
            var person = new Person
            {
                Score = 42
            };

            var result = ObjectPrinter.For<Person>()
                .Excluding(p => p.Score)
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
                .Printing(p => p.Age)
                .Using(value => $"Member: {value}")
                .PrintToString(person);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("Age = Member: 19"));
                Assert.That(result, Does.Contain("Score = Type: 42"));
            });
        }

        [Test]
        public void TrimmedToLength_AppliesAfterCustomFormatter()
        {
            var person = new Person
            {
                Name = "Alex"
            };

            var printer = ObjectPrinter.For<Person>();

            printer.Printing(p => p.Name)
                .Using(name => $"Hello, {name}!");

            printer.Printing(p => p.Name)
                .TrimmedToLength(9);

            var result = printer.PrintToString(person);

            Assert.That(result, Does.Contain("Name = Hello, Al"));
        }

        [Test]
        public void TrimmedToLength_RejectsNegativeLength()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                ObjectPrinter.For<Person>()
                    .Printing(p => p.Name)
                    .TrimmedToLength(-1));
        }

        [Test]
        public void Printing_RejectsNestedMemberExpression()
        {
            Assert.Throws<ArgumentException>(() =>
                ObjectPrinter.For<Person>()
                    .Printing(p => p.Name.Length));
        }

        [Test]
        public void Printing_RejectsExpressionWithoutMember()
        {
            Assert.Throws<ArgumentException>(() =>
                ObjectPrinter.For<Person>()
                    .Printing(p => 42));
        }

        [Test]
        public void PrintingType_UsesNullMarkerWhenFormatterTargetsUnderlyingType()
        {
            var record = new NullableAge();

            var result = ObjectPrinter.For<NullableAge>()
                .Printing<int>()
                .Using(age => $"#{age}")
                .PrintToString(record);

            var expected =
                "NullableAge" + PrintingTestFormat.NewLine +
                "\tAge = null" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintingType_PassesNullToNullableFormatter()
        {
            var record = new NullableAge();

            var result = ObjectPrinter.For<NullableAge>()
                .Printing<int?>()
                .Using(age => age.HasValue ? $"#{age.Value}" : "No age")
                .PrintToString(record);

            var expected =
                "NullableAge" + PrintingTestFormat.NewLine +
                "\tAge = No age" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        private sealed class NullableAge
        {
            public int? Age { get; set; }
        }

    }
}