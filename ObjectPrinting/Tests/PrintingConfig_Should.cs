using System;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class PrintingConfig_Should
    {
        private readonly Person objectForSerialization = new Person()
        {
            Id = Guid.Empty,
            Name = null,
            Height = 0.0,
            Age = 0,
            Child = new Person()
        };

        [Test]
        [TestCase(default(int), TestName = "int")]
        [TestCase(default(double), TestName = "double")]
        [TestCase("", TestName = "string")]
        public void Serialize_CertainTypePropertiesInAnAlternateWay<T>(T _)
        {
            var serialized = ObjectPrinter.For<Person>()
                .Serialize<T>()
                .Using(property => $"Property type {typeof(T)} serialized in an alternate way\r\n")
                .PrintToString(objectForSerialization);

            using (ApprovalTests.Namers.ApprovalResults.ForScenario(typeof(T)))
                Approvals.Verify(serialized);
        }

        [Test]
        public void Serialize_GUIDTypePropertiesInAnAlternateWay()
        {
            var serializedObject = ObjectPrinter.For<Person>()
                .Serialize<Guid>()
                .Using(property => $"Property type {typeof(Guid)} serialized in an alternate way\r\n")
                .PrintToString(objectForSerialization);

            Approvals.Verify(serializedObject);
        }

        [Test]
        public void Serialize_PersonTypePropertiesInAnAlternateWay()
        {
            var serializedObject = ObjectPrinter.For<Person>()
                .Serialize<Person>()
                .Using(property => $"Property type {typeof(Person)} serialized in an alternate way\r\n")
                .PrintToString(objectForSerialization);

            Approvals.Verify(serializedObject);
        }

        [Test]
        [TestCase(default(int), TestName = "int")]
        [TestCase(default(double), TestName = "double")]
        [TestCase("", TestName = "string")]
        public void Exclude_CertainTypePropertiesFromSerialization<T>(T _)
        {
            var serializedObject = ObjectPrinter.For<Person>()
                .ExcludingType<T>()
                .PrintToString(objectForSerialization);

            using (ApprovalTests.Namers.ApprovalResults.ForScenario(typeof(T)))
                Approvals.Verify(serializedObject);
        }

        [Test]
        public void Exclude_GUIDTypePropertiesFromSerialization()
        {
            var serializedObject = ObjectPrinter.For<Person>()
                .ExcludingType<Guid>()
                .PrintToString(objectForSerialization);

            Approvals.Verify(serializedObject);
        }

        [Test]
        public void Exclude_PersonTypePropertiesFromSerialization()
        {
            var serializedObject = ObjectPrinter.For<Person>()
                .ExcludingType<Person>()
                .PrintToString(objectForSerialization);

            Approvals.Verify(serializedObject);
        }
    }
}