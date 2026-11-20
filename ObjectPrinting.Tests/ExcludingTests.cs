using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests;

[TestFixture]
internal class ExcludingTests : PersonFixture
{
    [Test]
    public void Excluding_ByType_ShouldRemoveAllPropertiesOfThatType()
    {
        var result = person.PrintToString(cfg => cfg.Excluding<int>());

        result.Should().NotContain(nameof(person.Age)).And.NotContain(nameof(person.Address.Building));
    }

    [Test]
    public void Excluding_ByMember_ShouldRemoveOnlyThatMember()
    {
        var result = person.PrintToString(cfg => cfg.Excluding(p => p.Name));

        result.Should().NotContain(nameof(person.Name));
    }
}