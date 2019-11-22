using System;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterTests
    {
        [Test]
        public void DefaultSerialization()
        {
            var expectedSerialization = "Person" + Environment.NewLine
                                                 + '\t' + "Id = Guid" + Environment.NewLine
                                                 + '\t' + "Name = Vasiliy" + Environment.NewLine
                                                 + '\t' + "Height = 0" + Environment.NewLine
                                                 + '\t' + "Age = 19" + Environment.NewLine;
            var person = new Person {Name = "Vasiliy", Age = 19};

            var serialized = person.PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void TypeExclusion()
        {
            var expectedSerialization = "Person" + Environment.NewLine
                                                 + '\t' + "Name = Vasiliy" + Environment.NewLine
                                                 + '\t' + "Height = 0" + Environment.NewLine
                                                 + '\t' + "Age = 19" + Environment.NewLine;
            var person = new Person {Name = "Vasiliy", Age = 19};

            var serialized = person.Printing().Excluding<Guid>().PrintToString();

            serialized.Should().Be(expectedSerialization);
        }
    }
}