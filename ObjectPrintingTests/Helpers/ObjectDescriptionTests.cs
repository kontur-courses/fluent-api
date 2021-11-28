using System;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrintingTests.Helpers
{
    public class ObjectDescriptionTests
    {
        private readonly string n = Environment.NewLine;

        [Test]
        public void ToString_OneNestingLevel()
        {
            var description = new ObjectDescription("Person")
                .WithFields("Age", "Name", "Id");

            var actual = description.ToString();

            actual.Should().Be($"Person{n}"
                               + $"\tAge{n}"
                               + $"\tName{n}"
                               + "\tId");
        }

        [Test]
        public void ToString_TwoNestingLevels()
        {
            var description = new ObjectDescription("Person")
                .WithFields(new ObjectDescription("Address")
                    .WithFields("Street", "Zip"))
                .WithFields("Name", "Id");

            var actual = description.ToString();

            actual.Should().Be($"Person{n}"
                               + $"\tAddress{n}"
                               + $"\t\tStreet{n}"
                               + $"\t\tZip{n}"
                               + $"\tName{n}"
                               + "\tId");
        }

        [Test]
        public void ObjectDescription_Immutable()
        {
            var description = new ObjectDescription("Person");
            var descriptionWithName = description.WithFields("Name");
            var descriptionWithId = description.WithFields("Id");

            descriptionWithId.ToString()
                .Should().Be($"Person{n}"
                             + "\tId");
        }
    }
}