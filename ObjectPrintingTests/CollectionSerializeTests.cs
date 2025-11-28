using FluentAssertions;
using ObjectPrinting;
using ObjectPrinting.Test;

namespace ObjectPrintingTests;

[TestFixture]
public class CollectionSerializeTests
{
    [Test]
    public void PrintToString_ShouldSerializeListCorrectly()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        
        var result = ObjectPrinter.For<List<int>>().PrintToString(list);
        
        var expected = "[\r\n\t1,\r\n\t2,\r\n\t3,\r\n\t4,\r\n\t5\r\n]";
        result.Should().Be(expected);
    }

    [Test]
    public void PrintToString_ShouldSerializeEmptyList()
    {
        var list = new List<string>();
        
        var result = ObjectPrinter.For<List<string>>().PrintToString(list);
        
        result.Should().Be("[]");
    }

    [Test]
    public void PrintToString_ShouldSerializeArray()
    {
        var array = new[] { "a", "b", "c" };
        
        var result = ObjectPrinter.For<string[]>().PrintToString(array);
        
        var expected = "[\r\n\ta,\r\n\tb,\r\n\tc\r\n]";
        result.Should().Be(expected);
    }

    [Test]
    public void PrintToString_ShouldSerializeDictionary()
    {
        var dict = new Dictionary<string, int>
        {
            { "one", 1 },
            { "two", 2 }
        };
        
        var result = ObjectPrinter.For<Dictionary<string, int>>().PrintToString(dict);
        
        result.Should().Contain("one").And.Contain("1")
              .And.Contain("two").And.Contain("2")
              .And.Contain("[");
    }

    [Test]
    public void PrintToString_ShouldSerializeNestedCollections()
    {
        var matrix = new List<List<int>>
        {
            new() { 1, 2 },
            new() { 3, 4 }
        };
        
        var result = ObjectPrinter.For<List<List<int>>>().PrintToString(matrix);
        
        result.Should().Contain("1").And.Contain("2")
              .And.Contain("3").And.Contain("4")
              .And.Contain("[");
    }

    [Test]
    public void PrintToString_ShouldSerializeCollectionWithObjects()
    {
        var people = new List<Person>
        {
            new() { Name = "John", Age = 25 },
            new() { Name = "Jane", Age = 30 }
        };
        
        var result = ObjectPrinter.For<List<Person>>().PrintToString(people);
        
        result.Should().Contain("John").And.Contain("25")
              .And.Contain("Jane").And.Contain("30")
              .And.Contain("Person")
              .And.Contain("[");
    }

    [Test]
    public void PrintToString_ShouldHandleNullInCollections()
    {
        var listWithNulls = new List<string> { "a", null, "c" };
        
        var result = ObjectPrinter.For<List<string>>().PrintToString(listWithNulls);
        
        var expected = "[\r\n\ta,\r\n\tnull,\r\n\tc\r\n]";
        result.Should().Be(expected);
    }

    [Test]
    public void ExcludeType_ShouldWorkWithCollections()
    {
        var people = new List<Person>
        {
            new() { Name = "John", Age = 25 },
            new() { Name = "Jane", Age = 30 }
        };
        
        var result = ObjectPrinter.For<List<Person>>()
            .Exclude<string>()
            .PrintToString(people);
        
        result.Should().NotContain("Name =")
              .And.Contain("Age = 25")
              .And.Contain("Age = 30");
    }

    [Test]
    public void PrintSettings_ShouldApplyToCollectionElements()
    {
        var numbers = new List<int> { 10, 20, 30 };
        
        var result = ObjectPrinter.For<List<int>>()
            .PrintSettings<int>()
            .Using(i => $"#{i}#")
            .PrintToString(numbers);
        
        var expected = "[\r\n\t#10#,\r\n\t#20#,\r\n\t#30#\r\n]";
        result.Should().Be(expected);
    }
}