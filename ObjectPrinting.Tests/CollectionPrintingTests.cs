using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests;

[TestFixture]
internal class CollectionPrintingTests
{
    [Test]
    public void ShouldPrintListWithIndices()
    {
        var list = new[] { 10, 20, 30 };
        var result = list.PrintToString();

        result.Should().Contain("10")
            .And.Contain("20")
            .And.Contain("30");
    }
    
    [Test]
    public void ShouldPrintArrayWithIndices()
    {
        var array = new[] { "one", "two", "three" };
        var result = array.PrintToString();

        result.Should().Contain("one")
            .And.Contain("two")
            .And.Contain("three");
    }

    [Test]
    public void ShouldPrintDictionary()
    {
        var obj = new WithDictionary
        {
            Map = new Dictionary<string, int> { ["one"] = 1, ["two"] = 2 }
        };

        obj.PrintToString().Should().Contain("[one] = 1").And.Contain("[two] = 2");
    }
}