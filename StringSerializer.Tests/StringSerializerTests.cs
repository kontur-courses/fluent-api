using NUnit.Framework;
using ObjectPrinting;
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
            DoubleNumber = random.NextDouble(),
            Date = new DateTime(random.Next(1901, 2070), random.Next(1, 13), random.Next(1, 26)),
            Line = new string('_', random.Next(1, 100))
        };
        serializer = new StringSerializer<TestObject>();
    }

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        random = new Random();
    }

    private TestObject testObject;
    private StringSerializer<TestObject> serializer;
    private Random random;

    [Test]
    public void Serializer_Demo()
    {
        testObject.Dict = new Dictionary<int, TestObject>
        {
            [0] = new(),
            [1] = new(),
            [2] = new()
        };

        var serialized = serializer
            .Ignoring<DateTime>()
            .Ignoring(obj => obj.IntNumber)
            .ChangeSerializingFor(obj => obj.Line, line => $"<{line}>")
            .ChangeSerializingFor<double>(num => num.ToString(new CultureInfo("en")))
            .Serialize(testObject);

        Console.WriteLine(serialized);
        Assert.Pass();
    }

    [Test]
    public void Serializer_Should_IgnoreTypes()
    {
        var actual = serializer
            .Ignoring<int>()
            .Ignoring<double>()
            .Ignoring<string>()
            .Ignoring<DateTime>()
            .Ignoring<TestObject>()
            .Ignoring<Dictionary<int, TestObject>>()
            .Ignoring<List<string>>()
            .Ignoring<List<TestObject.InnerClass>>()
            .Serialize(testObject);

        Assert.AreEqual(actual, nameof(TestObject) + Environment.NewLine);
    }

    [Test]
    public void Serializer_Should_IgnoreMembers()
    {
        var actual = serializer
            .Ignoring(test => test.IntNumber)
            .Ignoring(test => test.DoubleNumber)
            .Ignoring(test => test.Line!)
            .Ignoring(test => test.Date)
            .Ignoring(test => test.CircularReference!)
            .Ignoring(test => test.Dict!)
            .Ignoring(test => test.List)
            .Ignoring(test => test.InnerClasses)
            .Serialize(testObject);

        Assert.AreEqual(actual, nameof(TestObject) + Environment.NewLine);
    }

    [TestCase("fr-FR")]
    [TestCase("ja-JP")]
    [TestCase("en-US")]
    [TestCase("ar-SA")]
    public void Serializer_Should_SupportCultureSetting(string signature)
    {
        var culture = new CultureInfo(signature);

        var actual = serializer
            .ChangeSerializingFor<DateTime>(date => date.ToString(culture))
            .ChangeSerializingFor<double>(num => num.ToString(culture))
            .Serialize(testObject);

        StringAssert.Contains(
            $"{nameof(TestObject.DoubleNumber)} = {testObject.DoubleNumber.ToString(culture)}", actual);
        StringAssert.Contains(
            $"{nameof(TestObject.Date)} = {testObject.Date.ToString(culture)}", actual);
    }

    [Test]
    public void Serializer_Should_SupportLineTrimming()
    {
        var cutLength = random.Next(10, 70);
        var actual = serializer
            .TrimLinesTo(cutLength)
            .Serialize(testObject);

        var expected = $"{nameof(TestObject.Line)} = {testObject.Line}";

        if (cutLength <= expected.Length)
            expected = expected[..cutLength];

        StringAssert.Contains(expected, actual);
    }

    [Test]
    public void Serializer_Should_SupportCustomSerializationForMembers()
    {
        var actual = serializer
            .ChangeSerializingFor(obj => obj.DoubleNumber, num => $"-> {num} <-")
            .ChangeSerializingFor(obj => obj.Line, _ => string.Empty)
            .Serialize(testObject);

        StringAssert.Contains(
            $"{nameof(TestObject.DoubleNumber)} = -> {testObject.DoubleNumber} <-", actual);
        StringAssert.Contains(
            $"{nameof(TestObject.CircularReference)} = {string.Empty}", actual);
    }

    [Test]
    public void Serializer_Should_SupportCustomSerializationForTypes()
    {
        var actual = serializer
            .ChangeSerializingFor<double>(num => $"-> {num} <-")
            .ChangeSerializingFor<string>(_ => string.Empty)
            .Serialize(testObject);

        StringAssert.Contains(
            $"{nameof(TestObject.DoubleNumber)} = -> {testObject.DoubleNumber} <-", actual);
        StringAssert.Contains(
            $"{nameof(TestObject.CircularReference)} = {string.Empty}", actual);
    }

    [Test]
    public void Serializer_Should_DetectCircularReferences()
    {
        testObject.CircularReference = testObject;
        serializer.Serialize(testObject);
        Assert.Pass();
    }

    [Test]
    public void Serializer_Should_StopAtFinalTypes()
    {
        var actual = serializer
            .Ignoring<List<TestObject.InnerClass>>()
            .Ignoring<TestObject>()
            .Ignoring<List<string>>()
            .Ignoring<Dictionary<int, TestObject>>()
            .Serialize(testObject);

        // \t\t will appear in actual only if final type will be opened.
        StringAssert.DoesNotContain("\t\t", actual);
    }

    [Test]
    public void Serializer_Should_WorkWithCollections()
    {
        var list = new List<int> { 1, 2, 3 };
        var actual = serializer.Serialize(list);

        Assert.AreEqual("[\n    1,\n    2,\n    3,\n]", actual);
    }

    [Test]
    public void Serializer_Should_WorkWithDictionaries()
    {
        var dict = new Dictionary<int, Dictionary<int, string>>
        {
            [0] = new() { [1] = "One" },
            [1] = new() { [2] = "Two" }
        };

        const string expected = "[\n    [0] => [\n\t    [1] => One,\n\t],\n    [1] => [\n\t    [2] => Two,\n\t],\n]";
        Assert.AreEqual(expected, serializer.Serialize(dict));
    }
}