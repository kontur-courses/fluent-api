using NUnit.Framework;
using ObjectPrinting.Extensions;
using StringSerializer.Tests.TestModels;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace StringSerializer.Tests;

[TestFixture]
public class StringSerializerTests
{
    [SetUp]
    public void SetUp()
    {
        testObject = new TestObject
        {
            IntNumber = random.Next(1, 1001),
            DoubleNumber = random.Next(1000),
            Date = new DateTime(random.Next(1901, 2070), random.Next(1, 13), random.Next(1, 26)),
            Line = new string('_', random.Next(1, 1000))
        };
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        random = new Random();
    }

    private TestObject testObject;
    private Random random;

    [Test]
    public void Serializer_MustBeAbleTo_IgnoreTypes()
    {
        var actual = testObject
            .IntoString(config => config
                .Ignoring<int>()
                .Ignoring<double>()
                .Ignoring<string>()
                .Ignoring<DateTime>()
                .Ignoring<TestObject>()
                .Ignoring<Dictionary<int, TestObject>>()
                .Ignoring<List<string>>());

        Assert.AreEqual(actual, nameof(TestObject) + Environment.NewLine);
    }

    [Test]
    public void Serializer_MustBeAbleTo_IgnoreMembers()
    {
        var actual = testObject
            .IntoString(config => config
                .Ignoring(test => test.IntNumber)
                .Ignoring(test => test.DoubleNumber)
                .Ignoring(test => test.Line!)
                .Ignoring(test => test.Date)
                .Ignoring(test => test.CircularReference!)
                .Ignoring(test => test.Dict!)
                .Ignoring(test => test.List));

        Assert.AreEqual(actual, nameof(TestObject) + Environment.NewLine);
    }

    [TestCase("fr-FR")]
    [TestCase("ja-JP")]
    [TestCase("en-US")]
    [TestCase("ar-SA")]
    public void Serializer_Must_SupportCultureSetting(string signature)
    {
        var culture = new CultureInfo(signature);

        var actual = testObject
            .IntoString(config => config
                .SetCultureTo<DateTime>(culture)
                .SetCultureTo<double>(culture));

        StringAssert.Contains(
            $"{nameof(TestObject.DoubleNumber)} = {testObject.DoubleNumber.ToString(culture)}", actual);
        StringAssert.Contains(
            $"{nameof(TestObject.Date)} = {testObject.Date.ToString(culture)}", actual);
    }

    [Test]
    public void Serializer_Must_SupportLineTrimming()
    {
        var cutLength = random.Next(1, testObject.Line!.Length);

        var actual = testObject
            .IntoString(config => config
                .TrimLinesTo(cutLength));

        StringAssert.Contains($"{nameof(TestObject.Line)} = {testObject.Line[..cutLength]}", actual);
    }

    [Test]
    public void Serializer_Must_SupportCustomSerializationForMembers()
    {
        var actual = testObject
            .IntoString(config => config
                .ChangeSerializingFor(obj => obj.DoubleNumber, num => $"-> {num} <-")
                .ChangeSerializingFor(obj => obj.Line, _ => string.Empty));

        StringAssert.Contains(
            $"{nameof(TestObject.DoubleNumber)} = -> {testObject.DoubleNumber} <-", actual);
        StringAssert.Contains(
            $"{nameof(TestObject.CircularReference)} = {string.Empty}", actual);
    }

    [Test]
    public void Serializer_Must_SupportCustomSerializationForTypes()
    {
        var actual = testObject
            .IntoString(config => config
                .ChangeSerializingFor<double>(num => $"-> {num} <-")
                .ChangeSerializingFor<string>(_ => string.Empty));

        StringAssert.Contains(
            $"{nameof(TestObject.DoubleNumber)} = -> {testObject.DoubleNumber} <-", actual);
        StringAssert.Contains(
            $"{nameof(TestObject.CircularReference)} = {string.Empty}", actual);
    }

    [Test]
    public void Serializer_Must_DetectCircularReferences()
    {
        testObject.CircularReference = testObject;
        testObject.IntoString();
        Assert.Pass();
    }
}