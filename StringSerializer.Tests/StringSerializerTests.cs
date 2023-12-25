using NUnit.Framework;
using ObjectPrinting.Extensions;
using StringSerializer.Tests.TestModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StringSerializer.Tests;

[TestFixture]
public class StringSerializerTests
{
    [SetUp]
    public void SetUp()
    {
        random = new Random();
        engine = new Engine("General Electric GEnx-2B67", 175000);
        airplane = new Airplane
        {
            ModelName = "Boing 747",
            Length = 70.66,
            Height = 19.40,
            MaxMass = 220130,
            MaxFlyingHeight = 13100,
            MaxSpeed = 956,
            Engine = engine,
            ProductionDate = new DateTime(1969, 2, 9)
        };
    }

    private Airplane? airplane;
    private Engine? engine;
    private Random? random;

    [Test]
    public void Serializer_MustBeAbleTo_IgnoreTypes()
    {
        var actual = CollectTokensFromLine(
            airplane.IntoString(config => config
                .Ignoring<int>()
                .Ignoring<double>()
                .Ignoring<string>()
                .Ignoring<DateTime>()));

        var expected = CollectTokensFromLine("Airplane Engine = Engine Ancestor = Null");

        CollectionAssert.AreEqual(actual, expected);
    }

    [Test]
    public void Serializer_MustBeAbleTo_IgnoreMembers()
    {
        var actual = CollectTokensFromLine(
            engine.IntoString(config => config.Ignoring(eng => eng!.Model)));

        var expected = CollectTokensFromLine("Engine HorsePower = 175000");

        CollectionAssert.AreEqual(actual, expected);
    }

    [Test]
    public void Serializer_MustBeAbleTo_IgnoreMembersAndTypesTogether()
    {
        var actual = CollectTokensFromLine(
            airplane.IntoString(
                config => config
                    .Ignoring(plane => plane!.Engine)
                    .Ignoring<int>()
                    .Ignoring<double>()
                    .Ignoring<string>()
                    .Ignoring(plane => plane!.Ancestor)));

        var expected = CollectTokensFromLine("Airplane ProductionDate = 09.02.1969 00:00:00");

        CollectionAssert.AreEqual(actual, expected);
    }

    [Test]
    public void Serializer_Must_SupportCultureSetting()
    {
        var number = random!.NextDouble();
        var cultures = new CultureInfo[] { new("ru-RU"), new("en-EN") };
        var randomCulture = cultures[random.Next(0, cultures.Length)];

        var actual = CollectTokensFromLine(
            new { Number = number }
                .IntoString(config => config
                    .SetCultureTo<double>(randomCulture)));

        Assert.That(actual.Last(), Is.EqualTo(number.ToString(randomCulture)));
    }

    [Test]
    public void Serializer_Must_SupportLineTrimming()
    {
        var testLine = new string('.', random!.Next(1, 101));
        var cuttingLength = random.Next(1, testLine.Length);

        var actual = CollectTokensFromLine(
            new { Line = testLine }
                .IntoString(config => config
                    .TrimLinesTo(cuttingLength)
                    .Ignoring<int>()));

        Assert.That(actual.Last(), Is.EqualTo(testLine[..cuttingLength]));
    }

    [Test]
    public void Serializer_Must_SupportCustomSerializationForTypes()
    {
        var actual = CollectTokensFromLine(
            airplane.IntoString(config => config
                .Ignoring<int>()
                .Ignoring<DateTime>()
                .Ignoring<Engine>()
                .Ignoring<string>()
                .Ignoring<Airplane>()
                .Ignoring(plane => plane!.MaxMass)
                .ChangeSerializingFor<double>(number => $"-> {number} <-")));

        var expected = CollectTokensFromLine("Airplane Length = -> 70,66 <- Height = -> 19,4 <-");

        CollectionAssert.AreEqual(actual, expected);
    }

    [Test]
    public void Serializer_Must_SupportCustomSerializationForMembers()
    {
        var actual = airplane.IntoString(config => config
            .Ignoring<int>()
            .Ignoring<double>()
            .Ignoring<string>()
            .Ignoring<DateTime>()
            .Ignoring(plane => plane!.Ancestor)
            .ChangeSerializingFor(
                plane => plane!.Engine,
                eng => $"{eng!.Model}: {eng.HorsePower}"));

        Assert.That(actual, Is.EqualTo("Airplane\n   Engine = General Electric GEnx-2B67: 175000\n"));
    }

    [Test]
    public void Serializer_Must_DetectCircularReferences()
    {
        airplane!.Ancestor = airplane;
        airplane.IntoString();
        Assert.Pass();
    }

    private static IEnumerable<string> CollectTokensFromLine(string line)
    {
        return line.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }
}