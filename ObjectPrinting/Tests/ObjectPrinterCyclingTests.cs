using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    class ObjectPrinterCyclingTests
    {
        [Test]
        public void PrintToString_WhenPropertyLinkToItselfShouldStop()
        {
            var node = new Node();
            node.neighbor = node;
            var expected = $"Node (Hash: {node.GetHashCode()})\r\n\tneighbor = <was above> (Hash: {node.GetHashCode()})\r\n";

            node.PrintToString().Should().Be(expected);
        }

        [Test]
        public void PrintToString_CycleOfThreeElements_ShouldStop()
        {
            var node = new Node{neighbor = new Node{neighbor = new Node()}};
            node.neighbor.neighbor = node;
            var expected =
                $"Node (Hash: {node.GetHashCode()})\r\n\tneighbor = Node (Hash: {node.neighbor.GetHashCode()})" +
                $"\r\n\t\tneighbor = <was above> (Hash: {node.GetHashCode()})\r\n\r\n";

            node.PrintToString().Should().Be(expected);
        }

        [Test]
        public void PrintToString_WhenTwoPropertyToOneObject_ShouldPrintBoth()
        {
            var nodes = new Node[2];
            nodes[0] = new Node();
            nodes[1] = nodes[0];
            var expected =
                $"{{\r\n\t0: Node (Hash: {nodes[0].GetHashCode()})\r\n\t\t\tneighbor = null\r\n\r\n\t1: Node " +
                $"(Hash: {nodes[1].GetHashCode()})\r\n\t\t\tneighbor = null\r\n\r\n}} (Hash: {nodes.GetHashCode()})";

            nodes.PrintToString().Should().Be(expected);
        }
    }
}
