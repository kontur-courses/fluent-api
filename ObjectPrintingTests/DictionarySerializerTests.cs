using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Serializers;
using ObjectPrintingTests.Helpers;
using ObjectPrintingTests.Persons;

namespace ObjectPrintingTests
{
    public class DictionarySerializerTests
    {
        private ISerializer serializer;

        [SetUp]
        public void SetUp()
        {
            serializer = new DictionarySerializer(new ObjectSerializer(new PrintingConfig()));
        }

        [TestCaseSource(nameof(CanSerializeTrueCases))]
        public void CanSerialize_True(object obj) =>
            serializer.CanSerialize(obj).Should().BeTrue();

        [Test]
        public void CanSerialize_False_WithObject() =>
            serializer.CanSerialize(new object()).Should().BeFalse();

        [Test]
        public void Serialize_Work_WithIntAsKeyAndValue()
        {
            var dict = new Dictionary<int, int>
            {
                [1] = 11,
                [2] = 12,
                [3] = 13,
            };

            var expected = string.Join(Environment.NewLine, "Dictionary[3]", "[1]: 11", "[2]: 12", "[3]: 13");

            serializer.Serialize(dict).ToString()
                .Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Serialize_Work_WithComplexObjectAsKey()
        {
            var dict = new Dictionary<Person, int> {[new Person()] = 0, [new Person()] = 1};
            var personDesc = PersonDescription.GetDefaultDescription(new Person())
                .WithExtraIndentation(1);

            var lines = new List<string> {"Dictionary[2]"};
            for (var i = 0; i < dict.Count; i++)
                lines.AddRange(new List<string> {"[", personDesc.ToString(), "]: ", $"\t{i}"});

            var expected = string.Join(Environment.NewLine, lines);

            var actual = serializer.Serialize(dict).ToString();

            actual.Should().Be(expected);
            Console.WriteLine(actual);
        }

        [Test]
        public void Serialize_Work_WithComplexObjectAsValue()
        {
            var dict = new Dictionary<int, Person> {[0] = new(), [1] = new()};
            var personDesc = PersonDescription.GetDefaultDescription(new Person())
                .WithOffset("[i]: ".Length);

            var expected = string.Join(Environment.NewLine,
                "Dictionary[2]",
                $"[0]: {personDesc}",
                $"[1]: {personDesc}");

            var actual = serializer.Serialize(dict).ToString();

            actual.Should().Be(expected);
            Console.WriteLine(actual);
        }

        [Test]
        public void Serialize_Work_WithComplexObjectAsKeyAndValue()
        {
            var dict = new Dictionary<Person, Person> {[new Person()] = new(), [new Person()] = new()};
            var personDesc = PersonDescription.GetDefaultDescription(new Person())
                .WithExtraIndentation(1);

            var lines = new List<string> {"Dictionary[2]"};
            for (var i = 0; i < dict.Count; i++)
                lines.AddRange(new List<string> {"[", personDesc.ToString(), "]: ", personDesc.ToString()});

            var expected = string.Join(Environment.NewLine, lines);

            var actual = serializer.Serialize(dict).ToString();

            actual.Should().Be(expected);
            Console.WriteLine(actual);
        }

        private static IEnumerable<TestCaseData> CanSerializeTrueCases()
        {
            yield return new TestCaseData(new Dictionary<int, int>()) {TestName = "Dictionary"};
            yield return new TestCaseData(new ReadOnlyDictionary<int, int>(new Dictionary<int, int>()))
                {TestName = "ReadOnlyDictionary"};
        }
    }
}