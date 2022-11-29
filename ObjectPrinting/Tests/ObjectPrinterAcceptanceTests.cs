using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;
        private PrintingConfig<Person> printer;

        [SetUp]
        public void SetUp()
        {
            person = new Person { Name = "Alex", Age = 19, Double = 2.2 };
            printer = ObjectPrinter.For<Person>();
        }

        [Test]
        public void ExcludeType()
        {
            var res = printer
                .Exclude<string>()
                .PrintToString(person);
            var expected = string.Join(Environment.NewLine,
                "Person", "\tId = Guid", "\tHeight = 0", "\tAge = 19", "\tDouble = 2,2", "");

            res.Should().Be(expected);
        }

        [Test]
        public void ExcludeName()
        {
            var res = printer
                .Exclude(e => e.Id)
                .PrintToString(person);
            var expected = string.Join(Environment.NewLine,
                 "Person", "\tName = Alex", "\tHeight = 0", "\tAge = 19", "\tDouble = 2,2", "");

            res.Should().Be(expected);
        }

        [Test]
        public void ChangeSerializationForType()
        {
            var res = printer
                .For<string>()
                    .SetSerialization(s => "строка с кастомной сериализацией")
                .PrintToString(person);
            var expected = string.Join(Environment.NewLine,
                "Person", "\tId = Guid", "\tName = строка с кастомной сериализацией", "\tHeight = 0", "\tAge = 19", "\tDouble = 2,2", "");

            res.Should().Be(expected);
        }

        [Test]
        public void ChangeSerializationForProperty()
        {
            var res = printer
                .For(e => e.Age)
                    .SetSerialization(num => "Изменил только Age")
                .PrintToString(person);
            var expected = string.Join(Environment.NewLine,
                "Person", "\tId = Guid", "\tName = Alex", "\tHeight = 0", "\tAge = Изменил только Age", "\tDouble = 2,2", "");

            res.Should().Be(expected);
        }

        [Test]
        public void TrimString()
        {
            var res = printer
                .For(e => e.Name)
                .TrimmedToLength(1)
                .PrintToString(person);
            var expected = string.Join(Environment.NewLine,
                "Person", "\tId = Guid", "\tName = A", "\tHeight = 0", "\tAge = 19", "\tDouble = 2,2", "");

            res.Should().Be(expected);
        }

        [Test]
        public void ChangeCulture()
        {
            var res = printer
                .For<double>()
                    .Using(System.Globalization.CultureInfo.GetCultureInfo("en-US"))
                .PrintToString(person);
            var expected = string.Join(Environment.NewLine,
                "Person", "\tId = Guid", "\tName = Alex", "\tHeight = 0", "\tAge = 19", "\tDouble = 2.2", "");

            res.Should().Be(expected);
        }
    }
}