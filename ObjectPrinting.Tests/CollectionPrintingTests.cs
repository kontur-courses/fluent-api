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
        var list = new[] { "lone", "topFloor", "darkLord" };
        var result = list.PrintToString();

        result.Should().Contain("0").And.Contain("lone")
            .And.Contain("1").And.Contain("topFloor")
            .And.Contain("2").And.Contain("darkLord");
    }
    
    [Test]
    public void ShouldPrintArrayWithIndices()
    {
        var array = new[] { "one", "two", "three" };
        var result = array.PrintToString();

        result.Should().Contain("0").And.Contain("one")
            .And.Contain("1").And.Contain("two")
            .And.Contain("2").And.Contain("three");
    }

    [Test]
    public void ShouldPrintDictionary()
    {
        var obj = new WithDictionary
        {
            Map = new Dictionary<string, int> { ["one"] = 1, ["two"] = 2 }
        };

        obj.PrintToString().Should().Contain("one").And.Contain("1")
            .And.Contain("two").And.Contain("2");
    }
}