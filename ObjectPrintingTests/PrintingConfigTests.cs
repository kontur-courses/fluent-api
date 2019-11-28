using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Configs;
using ObjectPrinting.Tests;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class PrintingConfigTests
    {
        [TestCase('b')]
        [TestCase(false)]
        [TestCase(true)]
        [TestCase((short) 314)]
        [TestCase((ushort) 314)]
        [TestCase(314)]
        [TestCase((uint) 314)]
        [TestCase((long) 314)]
        [TestCase((ulong) 314)]
        [TestCase(3.14159)]
        [TestCase(3.14159f)]
        [TestCase("this is string")]
        [TestCase("")]
        public void PrintToString_PrimitiveTypeObject_ShouldReturnValueOfToString<T>(T obj)
        {
            var printer = ObjectPrinter.For<T>();
            var expected = obj.ToString();

            var actual = printer.PrintToString(obj);

            actual.Should().BeEquivalentTo(expected);
        }


        private static IEnumerable<TestCaseData> GeneratePrimitiveTypeObjectAndSerializer()
        {
            yield return new TestCaseData("simple text", (Func<string, string>) (x => "other text"));

            yield return new TestCaseData(3.1415, (Func<double, string>) (x => $"{x / 2}"));

            yield return new TestCaseData(31415, (Func<int, string>) (x => $"{x / 2}"));

            yield return new TestCaseData(3.1415f, (Func<float, string>) (x => $"{x / 2}"));
        }

        [TestCaseSource(nameof(GeneratePrimitiveTypeObjectAndSerializer))]
        public void PrintToString_PrimitiveTypeObjectBySerializer_ShouldReturnResultOfSerializer<T>(
            T obj, Func<T, string> serializer)
        {
            var printer = ObjectPrinter.For<T>().Serializing<T>().Using(serializer);
            var expected = serializer(obj);

            var actual = printer.PrintToString(obj);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void Exclude_TypeWhichObjectPropertiesHaveNotContains_ShouldNotThrowException()
        {
            var printer = ObjectPrinter.For<Person>().Excluding<float>();
            var guid = Guid.NewGuid();
            var person = new Person
            {
                Id = guid,
                Name = "Alex",
                Surname = "Suvorov",
                Age = 45,
                Height = 172.6,
                Citizenship = "Russian"
            };
            var newLine = Environment.NewLine;
            var expected =
                $"Id = {guid}{newLine}" +
                $"Name = Alex{newLine}" +
                $"Surname = Suvorov{newLine}" +
                $"Height = 172.6{newLine}" +
                $"Age = 45{newLine}" +
                "Citizenship = Russian";

            var actual = printer.PrintToString(person);

            actual.Should().BeEquivalentTo(expected);
        }

        private static IEnumerable<TestCaseData> GenerateExcludedTypesAndSerializingResult()
        {
            yield return new TestCaseData(
                    string.Empty,
                    "Id = Guid\r\n\tHeight = 172,6\r\n\tAge = 45\r\n")
                .SetName("excluding all property with string type");

            yield return new TestCaseData(
                    0.0,
                    "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\t" +
                    "Surname = Suvorov\r\n\tAge = 45\r\n\tCitizenship = Russian\r\n")
                .SetName("excluding property with double type");

            yield return new TestCaseData(
                    new Guid(),
                    "Person\r\n\tName = Alex\r\n\t" +
                    "Surname = Suvorov\r\n\tHeight = 172,6\r\n\t" +
                    "Age = 45\r\n\tCitizenship = Russian\r\n")
                .SetName("excluding property with Guid type");

            yield return new TestCaseData(
                    42,
                    "Person\r\n\tId = Guid\r\n\tName = Alex\r\n\t" +
                    "Surname = Suvorov\r\n\tHeight = 172,6\r\n\t" +
                    "Citizenship = Russian\r\n")
                .SetName("excluding property with int type");
        }

        [TestCaseSource(nameof(GenerateExcludedTypesAndSerializingResult))]
        [Ignore("Not finished")]
        public void Exclude_Type_ShouldNotThrowException<T>(T _, string expected)
        {
            var printer = ObjectPrinter.For<Person>().Excluding<T>();
            var person = new Person
            {
                Id = new Guid(),
                Name = "Alex",
                Surname = "Suvorov",
                Age = 45,
                Height = 172.6,
                Citizenship = "Russian"
            };

            var actual = printer.PrintToString(person);

            actual.Should().BeEquivalentTo(expected);
        }

        private static IEnumerable<TestCaseData> GetObjectsVariousTypes()
        {
            yield return new TestCaseData(new Person());

            yield return new TestCaseData(new List<int>());

            yield return new TestCaseData(new Stack<string>());
        }

        [TestCase("text")]
        [TestCase(314)]
        [TestCase(3.14f)]
        [TestCase(3.14)]
        [TestCaseSource(nameof(GetObjectsVariousTypes))]
        public void Serializing_ForType_ShouldReturnRightContextForThisType<T>(T obj)
        {
            var printer = ObjectPrinter.For<T>();

            var expected = new PropertySerializingConfig<T, T>(printer);
            var actual = printer.Serializing<T>();

            actual.Should().BeOfType(expected.GetType());
        }

        [Test]
        public void PrintToString_WithExcludedProperty_ShouldNotPrintExcluded()
        {
            var person = new Person {Name = "Bob", Age = 42};
            var expected = $"Name = Bob{Environment.NewLine}Age = 42";

            var actual = ObjectPrinter
                .For<Person>()
                .Excluding(p => p.Citizenship)
                .Excluding(p => p.Surname)
                .Excluding(p => p.Id)
                .Excluding(p => p.Height)
                .PrintToString(person);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void PrintToString_WithCyclingReference_ShouldReturnRightString()
        {
            var person = new PersonWithParent {Name = "Bob", Age = 42};
            person.Parent = person;
            var newLine = Environment.NewLine;
            var expected = $"Name = Bob{newLine}Age = 42{newLine}Parent = [Cyclic reference detected]";

            var actual = ObjectPrinter
                .For<PersonWithParent>()
                .PrintToString(person);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void PrintToString_WithComplexType_ShouldReturnRightString()
        {
            var person1 = new PersonWithParent {Name = "Bob", Age = 42};
            var person2 = new PersonWithParent {Name = "Alice", Age = 42};
            var container = new PersonWithParentContainer {Person1 = person1, Person2 = person2};
            var expected = @"Person1
	Name = Bob
	Age = 42
	Parent = null
Person2
	Name = Alice
	Age = 42
	Parent = null";

            var actual = ObjectPrinter
                .For<PersonWithParentContainer>()
                .PrintToString(container);

            actual.Should().BeEquivalentTo(expected);
        }

        private static IEnumerable<TestCaseData> GenerateCollectionAndSerializingResult()
        {
            var newLine = Environment.NewLine;

            yield return new TestCaseData(new List<int> {3, 1, 4}, $"[3,{newLine}1,{newLine}4]{newLine}");

            yield return new TestCaseData(new HashSet<string> { "first", "second" }, $"[first,{newLine}second]{newLine}");

            yield return new TestCaseData(new Dictionary<double, bool>{[0.0] = true, [-3.14] = false}, 
                $"[Key = 0{newLine}Value = True,{newLine}Key = -3.14{newLine}Value = False]{newLine}");
        }

        [TestCaseSource(nameof(GenerateCollectionAndSerializingResult))]
        public void PrintToString_WithCollection_ShouldReturnRightString(IEnumerable enumerable, string expected)
        {
            var printer = ObjectPrinter.For<IEnumerable>();

            var actual = printer.PrintToString(enumerable);

            actual.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void PrintToString_ObjectWithCollectionProperty_ShouldReturnRightString()
        {
            var person1 = new Person() {Name = "Tom", Age = 14};
            var person2 = new Person() {Name = "Bob", Age = 13};
            var @class = new Class() {students = new List<Person>() {person1, person2}, classNumber = 7};
            var printer = ObjectPrinter.For<Class>().Excluding<double>().Excluding<Guid>();
            var newLine = Environment.NewLine;
            var expected = $"students = [Name = Tom{newLine}Surname = null{newLine}Age = 14{newLine}" +
                           $"Citizenship = null,{newLine}Name = Bob{newLine}Surname = null" +
                           $"{newLine}Age = 13{newLine}Citizenship = null]{newLine + newLine}classNumber = 7";

            var actual = printer.PrintToString(@class);

            actual.Should().BeEquivalentTo(expected);
        }
    }
}