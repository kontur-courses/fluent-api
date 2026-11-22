using NUnit.Framework;
using FluentAssertions;
using System.Collections;


namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrintingTests
{
    public static IEnumerable TestCases
    {
        get { yield return new TestCaseData<string, string>("", "").SetName("Test"); }
    }
    
    [TestCaseSource(nameof(TestCases))]
    public void ObjectPrinter_PrintsObjects(string obj, string expected)
    {
        var result = obj;
        result.Should().Be(expected);
    }
    
}