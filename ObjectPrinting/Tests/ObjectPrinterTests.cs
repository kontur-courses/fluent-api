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

        [Test]
        public void CustomTypeSerialization()
        {
            var person = new Person {Name = "Vasiliy", Age = 19};
            var guid = person.Id.ToString();
            var expectedSerialization = "Person" + Environment.NewLine
                                                 + '\t' + $"Id = {guid}" + Environment.NewLine
                                                 + '\t' + "Name = Vasiliy" + Environment.NewLine
                                                 + '\t' + "Height = 0" + Environment.NewLine
                                                 + '\t' + "Age = 19" + Environment.NewLine;

            var serialized = person.Printing().Printing<Guid>().Using(s => s.ToString()).PrintToString();

            serialized.Should().Be(expectedSerialization);
        }

        [Test]
        public void RedefinitionOfCustomTypeSerialization()
        {
            var person = new Person {Name = "Vasiliy", Age = 19};
            var expectedSerialization = "Person" + Environment.NewLine
                                                 + '\t' + "Id = Guid" + Environment.NewLine
                                                 + '\t' + "Name = Vasiliy" + Environment.NewLine
                                                 + '\t' + "Height = 0" + Environment.NewLine
                                                 + '\t' + "Age = 19" + Environment.NewLine;
            var serialized = person.Printing()
                .Printing<Guid>().Using(s => s.ToString())
                .Printing<Guid>().Using(s => s.GetType().Name)
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }
    }
}