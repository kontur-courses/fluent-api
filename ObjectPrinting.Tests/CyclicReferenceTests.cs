using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests;

[TestFixture]
public class CyclicReferenceTests
{
    [Test]
    public void ShouldDetectCycle()
    {
        var a = new Node { Name = "A" };
        var b = new Node { Name = "B", Next = a };
        a.Next = b;

        a.PrintToString().Should().Contain("cyclic reference to Node");
    }

    [Test]
    public void ShouldNotTreatSharedNonCyclicObjectAsCycle()
    {
        var shared = new Address { City = "Kazan" };
        var obj = new TwoAddresses { First = shared, Second = shared };

        var result = obj.PrintToString();

        result.Should().Contain("City = Kazan").And.NotContain("cyclic reference");
    }
}