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

        result.Should().Contain($"{nameof(person.Age)} = <{person.Age}>").And.Contain($"[0] = <{person.Scores[0]}>");
    }

    [Test]
    public void Using_ForMember_ShouldOverrideTypeSerializer()
    {
        var result = person.PrintToString(cfg => cfg
            .Printing<int>().Using(i => $"type:{i}")
            .Printing(p => p.Age).Using(i => $"member:{i}"));

        result.Should().Contain($"{nameof(person.Age)} = member:{person.Age}").And.Contain($"[0] = type:{person.Scores[0]}");
    }

    [Test]
    public void Using_Culture_ShouldFormatDate()
    {
        var result = person.PrintToString(cfg =>
            cfg.Printing(p => p.BirthDate).Using(new CultureInfo("ru-RU")));

        result.Should().Contain($"{nameof(person.BirthDate)} = {person.BirthDate.ToString(new CultureInfo("ru-RU"))}");
    }
}