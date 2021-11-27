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
        public void Constructor_ThrowException_IfObjectSerializerIsNull()
        {
            Assert.That(() => new ArraySerializer(null), Throws.InstanceOf<ArgumentException>());
        }

        [TestCaseSource(nameof(CanSerializeCases))]
        public bool CanSerialize(object obj) => serializer.CanSerialize(obj);

        [TestCase(1, TestName = "Not List")]
        [TestCase(null, TestName = "null")]
        public void Serialize_ThrowsException_IfObject(object obj)
        {
            Assert.That(() => serializer.Serialize(obj), Throws.InstanceOf<ArgumentException>());
        }


        [Test]
        public void Serialize_Work_WithInt()
        {
            var list = Enumerable.Range(1, 10).ToList();
            var expected = string.Join(Environment.NewLine, list.Select(i => $"[{i - 1}]: {i}").ToArray())
                           + Environment.NewLine;

            var actual = serializer.Serialize(list).ToString();

            Console.WriteLine(actual);
            actual.Should().Be(expected);
        }

        [Test]
        public void Serialize_Work_WithComplexObjects()
        {
            var person = new Person {Name = "Alex", Age = 19, Height = 2.1};
            var list = new List<Person> {person, person};
            var personDesc = new ObjectDescription(nameof(Person))
                .WithFields(
                    $"{nameof(Person.Id)} = 00000000-0000-0000-0000-000000000000",
                    $"{nameof(Person.Name)} = Alex",
                    $"{nameof(Person.Height)} = 2,1",
                    $"{nameof(Person.Age)} = 19"
                )
                .WithOffset("[i]: ".Length)
                .ToString();

            var expected = $"[0]: {personDesc}[1]: {personDesc}";

            var actual = serializer.Serialize(list).ToString();

            Console.WriteLine(actual);
            actual.Should().Be(expected);
        }

        private static IEnumerable<TestCaseData> CanSerializeCases()
        {
            yield return new TestCaseData(new List<int>()) {ExpectedResult = true, TestName = "True with List"};
            yield return new TestCaseData(new int[1]) {ExpectedResult = true, TestName = "True with Array"};
            yield return new TestCaseData(new object()) {ExpectedResult = false, TestName = "False with object"};
        }
    }
}