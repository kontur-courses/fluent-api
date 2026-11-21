using System.Globalization;
using FluentAssertions;
using ObjectPrinting.Tests.Models;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void PrintToString_Default_ShouldPrintAllPropertiesValues()
        {
            var person = GetDefaultPerson();

            var result = person.PrintToString();

            var expected = string.Join(Environment.NewLine,
                "Person",
                "\tId = " + person.Id,
                "\tName = " + person.Name,
                "\tLastName = " + person.LastName,
                "\tHeight = " + person.Height,
                "\tAge = " + person.Age,
                "");

            result.Should().Be(expected);
        }

        [TestCase(true, "True")]
        [TestCase(false, "False")]
        public void PrintToString_Boolean_ShouldPrintValue(bool value, string expected)
        {
            value.PrintToString().Should().Be(expected + Environment.NewLine);
        }

        [Test]
        public void PrintToString_DateTime_ShouldPrintValue()
        {
            var value = new DateTime(2026, 11, 19, 19, 11, 19);

            value.PrintToString().Should().Be(value.ToString(CultureInfo.CurrentCulture) + Environment.NewLine);
        }

        [Test]
        public void PrintToString_TimeSpan_ShouldPrintValue()
        {
            var value = new TimeSpan(1, 2, 3, 4);

            value.PrintToString().Should().Be("1.02:03:04" + Environment.NewLine);
        }

        [Test]
        public void PrintToString_ExcludingType_ShouldExcludeAllSameType()
        {
            var person = GetDefaultPerson();

            var result = person.PrintToString(c => c.Excluding<string>());

            result.Should()
                .NotBeEmpty()
                .And
                .NotContain("\tName = " + person.Name)
                .And
                .NotContain("\tLastName = " + person.LastName);
        }

        [Test]
        public void PrintToString_ExcludingProperty_ShouldExcludeProperty()
        {
            var person = GetDefaultPerson();

            var result = person.PrintToString(c => c.Excluding(p => p.Id));

            result.Should()
                .NotBeEmpty()
                .And
                .NotContain("\tId = " + person.Id);
        }

        [Test]
        public void PrintToString_ExcludingProperty_ShouldNotExcludeOtherPropertySameType()
        {
            var person = GetDefaultPerson();

            var result = person.PrintToString(c => c.Excluding(p => p.Name));

            result.Should()
                .Contain("\tLastName = " + person.LastName);
        }

        [Test]
        public void PrintToString_PrintingTypeWithUsing_ShouldPrintTypesViaLambda()
        {
            var person = GetDefaultPerson();

            var result = person.PrintToString(
                c => c.Printing<string>().Using(x => x + "wrap by using"));

            result.Should()
                .ContainAll("\tName = " + person.Name + "wrap by using", "\tLastName = " + person.LastName + "wrap by using");
        }

        [Test]
        public void PrintToString_PrintingPropertyWithUsing_ShouldPrintPropertyViaLambda()
        {
            var person = GetDefaultPerson();

            var result = person.PrintToString(
                c => c.Printing(p => p.Id).Using(x => x.ToString("X")));

            result.Should()
                .Contain("\tId = " + person.Id.ToString("X"));
        }

        [TestCase("en-US", "11/19/2026 11:19:11 AM")]
        [TestCase("ru-RU", "19.11.2026 11:19:11")]
        public void PrintToString_PrintingWithCultureInfo_ShouldPrintFormatType(
            string cultureName,
            string expected)
        {
            var date = new DateTime(2026 ,11, 19, 11, 19, 11);

            var result = date.PrintToString(
                c => c.Printing<DateTime>().Using(CultureInfo.GetCultureInfo(cultureName)));

            result.Should().Be(expected + Environment.NewLine);
        }

        [TestCase("Ale", 4, "Ale")]
        [TestCase("Alex", 4, "Alex")]
        [TestCase("Alexa", 4, "Alex")]
        [TestCase("Alex", 0, "")]
        [TestCase(null, 0, "null")]
        public void PrintToString_PrintingStringTrimmedToLength_ShouldPrintTrimmedString(
            string? name, int maxLen, string expectedName)
        {
            var person = GetDefaultPerson();
            person.Name = name!;

            var result = person.PrintToString(
                c => c.Printing(p => p.Name).TrimmedToLength(maxLen));

            result.Split(Environment.NewLine).Should()
                .Contain($"\tName = {expectedName}");
        }

        [Test]
        public void TrimmedToLength_WithNegativeLimit_ShouldThrowDuringConfiguration()
        {
            var propertyConfig = ObjectPrinter.For<Person>().Printing(p => p.Name);

            var exception = Assert.Throws<ArgumentOutOfRangeException>(
                () => propertyConfig.TrimmedToLength(-1));

            Assert.That(exception!.ParamName, Is.EqualTo("maxLen"));
        }

        [Test]
        public void PrintToString_WithCircularReference_ShouldPrintCircularReferenceMarker()
        {
            var loopNode = new LoopNode { Value = 1 };
            loopNode.Next = loopNode;

            var result = loopNode.PrintToString();

            result.Should().Contain("\tNext = [Circular reference]");
        }

        [Test]
        public void PrintToString_WithDistinctEqualObjects_ShouldPrintBothObjects()
        {
            var node = new LoopNode()
            {
                Value = 1,
                Next = new LoopNode { Value = 1 }
            };

            var result = node.PrintToString();

            var expected = string.Join(Environment.NewLine,
                "LoopNode",
                "\tValue = 1",
                "\tNext = LoopNode",
                "\t\tValue = 1",
                "\t\tNext = null",
                "\t\tPrevious = null",
                "\tPrevious = null",
                "");

            result.Should().Be(expected);
        }

        [Test]
        public void PrintToString_WithSharedReference_ShouldPrintBothOccurrences()
        {
            var loopNode = new LoopNode
            {
                Value = 1,
                Next = new LoopNode { Value = 2 }
            };
            loopNode.Previous = loopNode.Next;

            var expected = string.Join(Environment.NewLine,
                "LoopNode",
                "\tValue = 1",
                "\tNext = LoopNode",
                "\t\tValue = 2",
                "\t\tNext = null",
                "\t\tPrevious = null",
                "\tPrevious = LoopNode",
                "\t\tValue = 2",
                "\t\tNext = null",
                "\t\tPrevious = null",
                "");

            var result = loopNode.PrintToString();

            result.Should().Be(expected);
        }

        [Test]
        public void PrintToString_PrintingTypeAndPrintingPropertySameType_ShouldPrintViaPropertySerializer()
        {
            var person = GetDefaultPerson();

            var result = person.PrintToString(c => c
                .Printing<int>().Using(x => $"Число:{x}")
                .Printing(p => p.Age).Using(x => $"Возраст:{x}"));

            result.Should().Contain("\tAge = " + $"Возраст:{person.Age}");
        }

        [Test]
        public void PrintToString_PrintingTypeTwice_ShouldPrintViaLastLambda()
        {
            var person = GetDefaultPerson();

            var result = person.PrintToString(c => c
                .Printing<int>().Using(x => $"Число:{x}")
                .Printing<int>().Using(x => $"Digit:{x}"));

            result.Should().Contain("\tAge = " + $"Digit:{person.Age}");
        }

        [Test]
        public void PrintToString_Dictionary_ShouldPrintExpected()
        {
            var dictionary = new Dictionary<string, int> { ["apples"] = 3 };
            var expected = string.Join(Environment.NewLine,
                "Dictionary`2",
                "\tKeyValuePair`2",
                "\t\tKey = apples",
                "\t\tValue = 3",
                "");

            dictionary.PrintToString().Should().Be(expected);
        }

        [TestCaseSource(nameof(SequenceCases))]
        public void PrintToString_Sequence_ShouldPrintExpected(IEnumerable<int> sequence, string typeName)
        {
            var expected = string.Join(Environment.NewLine, typeName, "\t3", "\t1", "\t2", "");

            sequence.PrintToString().Should().Be(expected);
        }

        [Test]
        public void PrintToString_UsingTypeFormatter_ShouldFormatNestedCollectionItems()
        {
            var numbers = new[] { new[] { 3, 1 }, new[] { 2 } };
            var expected = string.Join(Environment.NewLine,
                "Int32[][]",
                "\tInt32[]",
                "\t\t[3]",
                "\t\t[1]",
                "\tInt32[]",
                "\t\t[2]",
                "");

            var result = numbers.PrintToString(c => c
                .Printing<int>().Using(number => $"[{number}]"));

            result.Should().Be(expected);
        }

        private static Person GetDefaultPerson() => new()
        {
            Name = "Alex",
            LastName = "Smith",
            Age = 19,
            Height = 1.85,
            Id = Guid.NewGuid()
        };

        private static IEnumerable<TestCaseData> SequenceCases()
        {
            yield return new TestCaseData(new[] { 3, 1, 2 }, "Int32[]").SetArgDisplayNames("array");
            yield return new TestCaseData(new List<int> { 3, 1, 2 }, "List`1").SetArgDisplayNames("list");
        }
    }
}
