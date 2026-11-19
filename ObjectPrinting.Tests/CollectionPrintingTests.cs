using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests;

[TestFixture]
internal class CollectionPrintingTests : PersonFixture
{
    [Test]
    public void ShouldPrintListWithIndices()
    {
        var result = person.PrintToString();

        result.Should().Contain("[0] = 10").And.Contain("[2] = 30");
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