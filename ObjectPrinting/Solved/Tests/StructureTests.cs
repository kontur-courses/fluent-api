using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Solved.Tests
{
    [TestFixture]
    public class StructureTests
    {
        [Test]
        public void Cyclic_Reference_Should_Not_Overflow()
        {
            var a = new Node { Name = "a" };
            var b = new Node { Name = "b", Next = a };
            a.Next = b;

            var act = () => a.PrintToString();

            act.Should().NotThrow();
            a.PrintToString().Should().Contain("<cycle>");
        }

        [Test]
        public void MaxDepth_Should_Collapse_Deep_Chain()
        {
            var me = new TreeNode { Name = "Я" };
            var dad = new TreeNode { Name = "Отец", Father = me };
            var grandpa = new TreeNode { Name = "Дед", Father = dad };
            var greatGrandpa = new TreeNode { Name = "Прадед", Father = grandpa };

            var s = ObjectPrinter
                .For<TreeNode>()
                .PrintToString(greatGrandpa, 3);

            s.Should().Contain("Прадед").And.Contain("Дед").And.Contain("Отец")
             .And.Contain("<TreeNode>").And.NotContain("Я");
        }

        [Test]
        public void List_Should_Be_Printed()
        {
            var list = new List<int> { 10, 20, 30 };

            var s = list.PrintToString();

            s.Should().Contain("10").And.Contain("20").And.Contain("30");
        }

        [Test]
        public void Dictionary_Should_Be_Printed()
        {
            var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
            
            var s = dict.PrintToString();

            s.Should().Contain("[a] = 1").And.Contain("[b] = 2");
        }
    }
}