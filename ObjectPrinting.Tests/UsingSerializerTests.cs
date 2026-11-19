using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests;

[TestFixture]
internal class UsingSerializerTests : PersonFixture
{
    [Test]
    public void Using_ForType_ShouldApplyToAllMembersOfThatType()
    {
        var result = person.PrintToString(cfg =>
            cfg.Printing<int>().Using(i => $"<{i}>"));

        result.Should().Contain("Age = <42>").And.Contain("[0] = <10>");
    }

    [Test]
    public void Using_ForMember_ShouldOverrideTypeSerializer()
    {
        var result = person.PrintToString(cfg => cfg
            .Printing<int>().Using(i => $"type:{i}")
            .Printing(p => p.Age).Using(i => $"member:{i}"));

        result.Should().Contain("Age = member:42").And.Contain("[0] = type:10");
    }

    [Test]
    public void Using_Culture_ShouldFormatDate()
    {
        var result = person.PrintToString(cfg =>
            cfg.Printing(p => p.BirthDate).Using(new CultureInfo("ru-RU")));

        result.Should().Contain("BirthDate = 12.05.1980");
    }
}