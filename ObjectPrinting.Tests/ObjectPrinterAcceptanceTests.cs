using System;
using NUnit.Framework;
using System.Globalization;
using System.Threading.Tasks;
using VerifyNUnit;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public async Task Demo()
        {
            var person = new Person
            {
                Name = "Alexandra",
                Age = 19,
                Height = 1.75,
                Score = 73
            };

            var printer = ObjectPrinter.For<Person>();

            printer.Excluding<System.Guid>();

            printer.Printing<int>()
                .Using(value => $"#{value}");

            printer.Printing<double>()
                .Using(CultureInfo.GetCultureInfo("fr-FR"));

            printer.Printing(p => p.Name)
                .Using(name => name.ToUpperInvariant());

            printer.Printing(p => p.Name)
                .TrimmedToLength(5);

            printer.Excluding(p => p.Score);

            var result = printer.PrintToString(person);

            await Verifier.Verify(result);
        }

        [Test]
        public void PrintToString_PrintsPublicProperties()
        {
            var record = new PublicPropertyRecord();

            var result = ObjectPrinter.For<PublicPropertyRecord>()
                .PrintToString(record);

            var expected =
                "PublicPropertyRecord" + PrintingTestFormat.NewLine +
                "\tName = Alex" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_PrintsPublicFields()
        {
            var record = new PublicFieldRecord();

            var result = ObjectPrinter.For<PublicFieldRecord>()
                .PrintToString(record);

            var expected =
                "PublicFieldRecord" + PrintingTestFormat.NewLine +
                "\tScore = 73" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void ExcludingType_DoesNotPrintMembersOfThatType()
        {
            var person = new Person
            {
                Name = "Alex",
                Age = 19,
                Id = Guid.NewGuid(),
                ExternalId = Guid.NewGuid()
            };

            var result = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .PrintToString(person);

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Not.Contain("Id = "));
                Assert.That(result, Does.Not.Contain("ExternalId = "));

                Assert.That(result, Does.Contain("Name = Alex"));
                Assert.That(result, Does.Contain("Age = 19"));
            });
        }

        [Test]
        public void ExcludingType_DoesNotInvokeExcludedPropertyGetter()
        {
            var person = new PersonWithTrackedId();

            var result = ObjectPrinter.For<PersonWithTrackedId>()
                .Excluding<Guid>()
                .PrintToString(person);

            Assert.That(person.IdGetterCalls, Is.Zero);
            Assert.That(result, Does.Not.Contain("Id = "));
        }

        private sealed class PersonWithTrackedId
        {
            public int IdGetterCalls { get; private set; }

            public Guid Id
            {
                get
                {
                    IdGetterCalls++;
                    return Guid.NewGuid();
                }
            }
        }

        private sealed class PublicPropertyRecord
        {
            public string Name { get; } = "Alex";
        }

        private sealed class PublicFieldRecord
        {
            public int Score = 73;
        }
    }
}