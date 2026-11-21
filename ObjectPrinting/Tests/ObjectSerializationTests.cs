using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using static ObjectPrinting.Tests.TestOutput;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectSerializationTests
{
    [TestCaseSource(nameof(FinalTypeCases))]
    public void FinalTypes_SerializeToSingleLine(object? value, string expected)
    {
        var result = value.PrintToString();

        result.Should().Be(expected);
    }

    [TestCaseSource(nameof(ComplexObjectCases))]
    public void ComplexObjects_SerializeMembersAndNesting(object value, string expected)
    {
        var result = value.PrintToString();

        result.Should().Be(expected);
    }

    [TestCaseSource(nameof(ReferenceGraphCases))]
    public void ReferenceGraphs_DetectCyclesWithoutFalsePositives(Func<string> serialize, string expected)
    {
        var result = serialize();

        result.Should().Be(expected);
    }

    private static IEnumerable<TestCaseData> FinalTypeCases()
    {
        var guid = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var timeSpan = TimeSpan.FromMinutes(90);
        var dateOnly = new DateOnly(2025, 5, 10);
        var timeOnly = new TimeOnly(14, 30, 0);
        var dateTime = new DateTime(2025, 5, 10, 14, 30, 0);

        yield return new TestCaseData(null, Lines("null"))
            .SetName("FinalType_Null");

        yield return new TestCaseData(42, Lines("42"))
            .SetName("FinalType_Primitive_Int");

        yield return new TestCaseData(true, Lines("True"))
            .SetName("FinalType_Primitive_Bool");

        yield return new TestCaseData(DayOfWeek.Monday, Lines("Monday"))
            .SetName("FinalType_Enum");

        yield return new TestCaseData("hello", Lines("hello"))
            .SetName("FinalType_String");

        yield return new TestCaseData(12m, Lines("12"))
            .SetName("FinalType_Decimal");

        yield return new TestCaseData(guid, Lines(guid.ToString()))
            .SetName("FinalType_Guid");

        yield return new TestCaseData(timeSpan, Lines(timeSpan.ToString()))
            .SetName("FinalType_TimeSpan");

        yield return new TestCaseData(dateOnly, Lines(dateOnly.ToString()))
            .SetName("FinalType_DateOnly");

        yield return new TestCaseData(timeOnly, Lines(timeOnly.ToString()))
            .SetName("FinalType_TimeOnly");

        yield return new TestCaseData(dateTime, Lines(dateTime.ToString()))
            .SetName("FinalType_DateTime");
    }

    private static IEnumerable<TestCaseData> ComplexObjectCases()
    {
        yield return new TestCaseData(
                new MixedVisibilityObject(),
                Lines(
                    nameof(MixedVisibilityObject),
                    "\tPublicProperty = visible",
                    "\tPublicField = 42"))
            .SetName("ComplexObject_OnlyPublicReadableInstanceMembers");

        yield return new TestCaseData(
                new OuterObject { Title = "Root", Location = new Point(1, 2) },
                Lines(
                    nameof(OuterObject),
                    "\tTitle = Root",
                    "\tLocation = Point",
                    "\t\tX = 1",
                    "\t\tY = 2"))
            .SetName("ComplexObject_NestedObjectIndentation");
    }

    private static IEnumerable<TestCaseData> ReferenceGraphCases()
    {
        yield return new TestCaseData(
                () =>
                {
                    var node = new GraphNode { Name = "Self" };
                    node.Left = node;
                    return node.PrintToString();
                },
                Lines(
                    nameof(GraphNode),
                    "\tName = Self",
                    "\tLeft = <cyclic reference>",
                    "\tRight = null"))
            .SetName("Cycle_DirectSelfReference");

        yield return new TestCaseData(
                () =>
                {
                    var a = new GraphNode { Name = "A" };
                    var b = new GraphNode { Name = "B" };
                    a.Left = b;
                    b.Left = a;
                    return a.PrintToString();
                },
                Lines(
                    nameof(GraphNode),
                    "\tName = A",
                    "\tLeft = GraphNode",
                    "\t\tName = B",
                    "\t\tLeft = <cyclic reference>",
                    "\t\tRight = null",
                    "\tRight = null"))
            .SetName("Cycle_MutualReference_A_To_B_To_A");

        yield return new TestCaseData(
                () =>
                {
                    var shared = new GraphNode { Name = "Leaf" };
                    var root = new GraphNode { Name = "Root", Left = shared, Right = shared };
                    return root.PrintToString();
                },
                Lines(
                    nameof(GraphNode),
                    "\tName = Root",
                    "\tLeft = GraphNode",
                    "\t\tName = Leaf",
                    "\t\tLeft = null",
                    "\t\tRight = null",
                    "\tRight = GraphNode",
                    "\t\tName = Leaf",
                    "\t\tLeft = null",
                    "\t\tRight = null"))
            .SetName("NonCycle_SharedSiblingReference");

        yield return new TestCaseData(
                () => new StructHolder { First = new Point(1, 2), Second = new Point(1, 2) }.PrintToString(),
                Lines(
                    nameof(StructHolder),
                    "\tFirst = Point",
                    "\t\tX = 1",
                    "\t\tY = 2",
                    "\tSecond = Point",
                    "\t\tX = 1",
                    "\t\tY = 2"))
            .SetName("NonCycle_EqualStructValues");
    }

    private class MixedVisibilityObject
    {
        public int PublicField = 42;
        public static string StaticProperty => "static";
        public string PublicProperty { get; set; } = "visible";

        public string WriteOnlyProperty
        {
            set => _ = value;
        }

        public string this[int index] => $"item-{index}";
        private int PrivateProperty { get; } = 100;
    }

    private class OuterObject
    {
        public string Title { get; set; } = string.Empty;
        public Point Location { get; set; }
    }

    private class GraphNode
    {
        public string Name { get; set; } = string.Empty;
        public GraphNode? Left { get; set; }
        public GraphNode? Right { get; set; }
    }

    private struct Point(int x, int y)
    {
        public int X { get; } = x;
        public int Y { get; } = y;
    }

    private class StructHolder
    {
        public Point First { get; set; }
        public Point Second { get; set; }
    }
}
