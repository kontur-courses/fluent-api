using System;
using System.Globalization;
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
        public void MultipleTypesExclusion()
        {
            var expectedSerialization = "Person" + Environment.NewLine
                                                 + '\t' + "Height = 0" + Environment.NewLine
                                                 + '\t' + "Age = 19" + Environment.NewLine;
            var person = new Person {Name = "Vasiliy", Age = 19};

            var serialized = person.Printing()
                .Excluding<Guid>()
                .Excluding<string>()
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }
        
        [Test]
        public void TypeExclusionOfExcludedType()
        {
            var expectedSerialization = "Person" + Environment.NewLine
                                                 + '\t' + "Name = Vasiliy" + Environment.NewLine
                                                 + '\t' + "Height = 0" + Environment.NewLine
                                                 + '\t' + "Age = 19" + Environment.NewLine;
            var person = new Person {Name = "Vasiliy", Age = 19};

            var serialized = person.Printing()
                .Excluding<Guid>()
                .Excluding<Guid>()
                .PrintToString();

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
        public void CustomTypeSerializationPrecedenceOverFinalTypes()
        {
            var person = new Person {Name = "Vasiliy", Age = 19};
            var expectedSerialization = "Person" + Environment.NewLine
                                                 + '\t' + $"Id = Guid" + Environment.NewLine
                                                 + '\t' + "Name = Vasiliy" + Environment.NewLine
                                                 + '\t' + "Height = 0" + Environment.NewLine
                                                 + '\t' + "Age = NO" + Environment.NewLine;

            var serialized = person.Printing()
                .Printing<int>().Using(s => "NO")
                .PrintToString();

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

        [Test]
        public void CultureInfoForNumericTypes_RussianCulture()
        {
            var person = new Person {Name = "Vasiliy", Age = 19, Height = 179.5d};
            var expectedSerialization = "Person" + Environment.NewLine
                                                 + '\t' + "Id = Guid" + Environment.NewLine
                                                 + '\t' + "Name = Vasiliy" + Environment.NewLine
                                                 + '\t' + "Height = 179,5" + Environment.NewLine
                                                 + '\t' + "Age = 19" + Environment.NewLine;

            var serialized = person.Printing()
                .Printing<double>().Using(CultureInfo.CreateSpecificCulture("ru-ru"))
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }
        
        [Test]
        public void CultureInfoForNumericTypes_InvariantCulture()
        {
            var person = new Person {Name = "Vasiliy", Age = 19, Height = 181.7d};
            var expectedSerialization = "Person" + Environment.NewLine
                                                 + '\t' + "Id = Guid" + Environment.NewLine
                                                 + '\t' + "Name = Vasiliy" + Environment.NewLine
                                                 + '\t' + "Height = 181.7" + Environment.NewLine
                                                 + '\t' + "Age = 19" + Environment.NewLine;

            var serialized = person.Printing()
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                .PrintToString();

            serialized.Should().Be(expectedSerialization);
        }
    }
}