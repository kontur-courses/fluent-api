using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Serializers;
using ObjectPrintingTests.Helpers;
using ObjectPrintingTests.Persons;

namespace ObjectPrintingTests
{
    public class ArraySerializerTests
    {
        private ObjectSerializer objectSerializer;
        private ArraySerializer serializer;

        [SetUp]
        public void SetUp()
        {
            objectSerializer = new ObjectSerializer(new PrintingConfig());
            serializer = new ArraySerializer(objectSerializer);
        }

        [Test]
        public void Constructor_ThrowException_IfObjectSerializerIsNull() =>
            Assert.That(() => new ArraySerializer(null), Throws.InstanceOf<ArgumentException>());

        [TestCaseSource(nameof(CanSerializeTrueCases))]
        public void CanSerialize_True(object obj) => 
            serializer.CanSerialize(obj).Should().BeTrue();

        [Test]
        public void CanSerialize_False_WithObject() =>
            serializer.CanSerialize(new object()).Should().BeFalse();

        [TestCase(1, TestName = "Not List")]
        [TestCase(null, TestName = "null")]
        public void Serialize_ThrowsException_IfObject(object obj) =>
            Assert.That(() => serializer.Serialize(obj), Throws.InstanceOf<ArgumentException>());

        [Test]
        public void Serialize_Work_WithReadonlyList()
        {
            var list = Enumerable.Range(1, 10).ToList().AsReadOnly();
            var expected = string.Join(Environment.NewLine,
                new[] {"List[10]"}
                    .Concat(list.Select(i => $"[{i - 1}]: {i}")));

            var actual = serializer.Serialize(list).ToString();

            actual.Should().Be(expected);
            Console.WriteLine(actual);
        }

        [Test]
        public void Serialize_Work_WithComplexObjects()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 2.1};
            var list = new List<Person> {person, person};
            var personDesc = PersonDescription.GetDefaultDescription(new Person())
                .WithOffset("[i]: ".Length);

            var expected = string.Join(Environment.NewLine,
                "List[2]",
                $"[0]: {personDesc}",
                $"[1]: {personDesc}");

            var actual = serializer.Serialize(list).ToString();

            actual.Should().Be(expected);
            Console.WriteLine(actual);
        }

        private static IEnumerable<TestCaseData> CanSerializeTrueCases()
        {
            yield return new TestCaseData(new List<int>()) {TestName = "List"};
            yield return new TestCaseData(new int[1]) {TestName = "Array"};
            yield return new TestCaseData(new List<int>().AsReadOnly()) {TestName = "ReadOnlyList"};
        }
    }
}