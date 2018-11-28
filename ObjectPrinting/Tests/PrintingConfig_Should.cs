using System;
using System.Globalization;
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
            Name = "Mark",
            Height = 527.963598,
            Age = 0,
            Child = new Person()
            {
                Id = Guid.Empty,
                Name = "Ford",
                Height = 0.0,
                Age = 0
            }
        };

        [Test]
        public void Exclude_IdPropertyFromSerialization()
        {
            var serialized = ObjectPrinter.For<Person>()
                .ExcludingProperty(person => person.Id)
                .PrintToString(objectForSerialization);

            Approvals.Verify(serialized);
        }

        [Test]
        public void Exclude_NamePropertyFromSerialization()
        {
            var serialized = ObjectPrinter.For<Person>()
                .ExcludingProperty(person => person.Name)
                .PrintToString(objectForSerialization);

            Approvals.Verify(serialized);
        }

        [Test]
        public void Exclude_AgePropertyFromSerialization()
        {
            var serialized = ObjectPrinter.For<Person>()
                .ExcludingProperty(person => person.Age)
                .PrintToString(objectForSerialization);

            Approvals.Verify(serialized);
        }

        [Test]
        public void Exclude_HeightPropertyFromSerialization()
        {
            var serialized = ObjectPrinter.For<Person>()
                .ExcludingProperty(person => person.Height)
                .PrintToString(objectForSerialization);

            Approvals.Verify(serialized);
        }

        [Test]
        public void Exclude_ChildPropertyFromSerialization()
        {
            var serialized = ObjectPrinter.For<Person>()
                .ExcludingProperty(person => person.Child)
                .PrintToString(objectForSerialization);

            Approvals.Verify(serialized);
        }

        [Test]
        [TestCase(1, TestName = "1")]
        [TestCase(10, TestName = "10")]
        public void Serialize_TrimmingToLength(int maxLength)
        {
            var serialized = ObjectPrinter.For<Person>()
                .Serialize<string>()
                .TrimmingToLength(maxLength)
                .PrintToString(objectForSerialization);

            using (ApprovalTests.Namers.ApprovalResults.ForScenario(maxLength.ToString()))
                Approvals.Verify(serialized);
        }

        [Test]
        public void Serialize_IdPropertyInAnAlternateWay()
        {
            var serialized = ObjectPrinter.For<Person>()
                .Serialize(person => person.Id)
                .Using(id => "Specific property Id serialized in an alternate way\r\n")
                .PrintToString(objectForSerialization);

            Approvals.Verify(serialized);
        }

        [Test]
        public void Serialize_NamePropertyInAnAlternateWay()
        {
            var serialized = ObjectPrinter.For<Person>()
                .Serialize(person => person.Name)
                .Using(name => "Specific property Name serialized in an alternate way\r\n")
                .PrintToString(objectForSerialization);

            Approvals.Verify(serialized);
        }

        [Test]
        public void Serialize_AgePropertyInAnAlternateWay()
        {
            var serialized = ObjectPrinter.For<Person>()
                .Serialize(person => person.Age)
                .Using(age => "Specific property Age serialized in an alternate way\r\n")
                .PrintToString(objectForSerialization);

            Approvals.Verify(serialized);
        }

        [Test]
        public void Serialize_HeightPropertyInAnAlternateWay()
        {
            var serialized = ObjectPrinter.For<Person>()
                .Serialize(person => person.Height)
                .Using(height => "Specific property Height serialized in an alternate way\r\n")
                .PrintToString(objectForSerialization);

            Approvals.Verify(serialized);
        }

        [Test]
        public void Serialize_ChildPropertyInAnAlternateWay()
        {
            var serialized = ObjectPrinter.For<Person>()
                .Serialize(person => person.Child)
                .Using(child => "Specific property Child serialized in an alternate way\r\n")
                .PrintToString(objectForSerialization);

            Approvals.Verify(serialized);
        }

        [Test]
        [TestCase("ru-RU", TestName = "Russian")]
        [TestCase("en-US", TestName = "English US")]
        public void Serialize_DoubleTypePropertiesWithRussianCulture(string culture)
        {
            var serialized = ObjectPrinter.For<Person>()
                .Serialize<double>()
                .Using(CultureInfo.GetCultureInfo(culture))
                .PrintToString(objectForSerialization);

            using (ApprovalTests.Namers.ApprovalResults.ForScenario(culture))
                Approvals.Verify(serialized);
        }

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