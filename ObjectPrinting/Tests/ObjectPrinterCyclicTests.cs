using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    class ObjectPrinterCyclicTests
    {
        [Test]
        public void PrintToString_WhenPropertyLinkToItselfShouldStop()
        {
            var node = new Node();
            node.Neighbor = node;
            var expected = $"Node (Hash: {node.GetHashCode()})\r\n\tneighbor = <was above> (Hash: {node.GetHashCode()})\r\n";

            node.PrintToString().Should().Be(expected);
        }

        [Test]
        public void PrintToString_CycleOfThreeElements_ShouldStop()
        {
            var node = new Node{Neighbor = new Node{Neighbor = new Node()}};
            node.Neighbor.Neighbor = node;
            var expected =
                $"Node (Hash: {node.GetHashCode()})\r\n\tneighbor = Node (Hash: {node.Neighbor.GetHashCode()})" +
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
