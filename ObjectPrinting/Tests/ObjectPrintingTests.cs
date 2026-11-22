using NUnit.Framework;
using FluentAssertions;
using System.Collections;


namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrintingTests
{
    public class TestAddress
    {
        public string City { get; set; } = "";
    }

    public class TestPerson
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public TestAddress? Address { get; set; }
    }

    public class TestNode
    {
        var result = obj;
        public string Name { get; set; } = "";
        public TestNode? Next { get; set; }
    }
    public static IEnumerable CycleCases
    {
        get
        {
            var self = new TestNode { Name = "A" };
            self.Next = self;

            yield return new TestCaseData<TestNode, string>(self,
                "TestNode" + Environment.NewLine + "\tName = A" + Environment.NewLine + "\tNext = <Cycle>" +
                Environment.NewLine).SetName("Cycle: object referencing to itself");

            var a = new TestNode { Name = "A" };
            var b = new TestNode { Name = "B" };
            var c = new TestNode { Name = "C" };
            a.Next = b;
            b.Next = c;
            c.Next = a;

            yield return new TestCaseData<TestNode, string>(a,
                "TestNode" + Environment.NewLine + "\tName = A" + Environment.NewLine + "\tNext = TestNode" +
                Environment.NewLine + "\t\tName = B" + Environment.NewLine + "\t\tNext = TestNode" +
                Environment.NewLine + "\t\t\tName = C" + Environment.NewLine + "\t\t\tNext = <Cycle>" +
                Environment.NewLine).SetName("Cycle: three objects forming a loop");
        }
    }
    [TestCaseSource(nameof(CycleCases))]
    public void ObjectPrinter_HandlesCycles(TestNode obj, string expected)
    {
        var result = obj.PrintToString();

        result.Should().Be(expected);
    }
    
}