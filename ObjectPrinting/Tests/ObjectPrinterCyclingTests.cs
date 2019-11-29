using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    class ObjectPrinterCyclingTests
    {
        [Test]
        public void PrintToString_WhenPropertyLinkToItself()
        {
            var node = new Node();
            node.neighbor = node;
            var expected = $"Node (Hash: {node.GetHashCode()})\r\n\tneighbor = <was above> (Hash: {node.GetHashCode()})\r\n";

            node.PrintToString().Should().Be(expected);
        }

        [Test]
        public void PrintToString_CycleOfThreeElements()
        {
            var node = new Node{neighbor = new Node{neighbor = new Node()}};
            node.neighbor.neighbor = node;
            var expected =
                $"Node (Hash: {node.GetHashCode()})\r\n\tneighbor = Node (Hash: {node.neighbor.GetHashCode()})\r\n\t\tneighbor = <was above> (Hash: {node.GetHashCode()})\r\n\r\n";

            node.PrintToString().Should().Be(expected);
        }
    }
}
