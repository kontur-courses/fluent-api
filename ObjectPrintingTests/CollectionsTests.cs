using ObjectPrinting;

namespace ObjectPrintingTests;

public class CollectionsTests
{
    private class TestDictionary
    {
        public Dictionary<string, int> DictionaryProperty { get; set; }
    }

    [Test]
    public void PrintToString_WithDictionary()
    {
        var obj = new TestDictionary
        {
            DictionaryProperty = new Dictionary<string, int>
            {
                { "One", 1 },
                { "Two", 2 },
                { "Three", 3 }
            }
        };
        
        var printer = ObjectPrinter.For<TestDictionary>();
        var result = printer.PrintToString(obj);

        result.Should().Be("TestDictionary:" +
                           "\nOne = 1" +
                           "\nTwo = 2" +
                           "\nThree = 3");
    }
    
    private class TestArray
    {
        public int[] ArrayProperty { get; set; }
    }

    [Test]
    public void PrintToString_WithArray()
    {
        var obj = new TestArray
        {
            ArrayProperty = new[] { 10, 20, 30 }
        };
        var printingConfig = new PrintingConfig<TestArray>();
        var result = printingConfig.PrintToString(obj);

        result.Should().Be("TestArray:" +
                           "\n0 = 10" +
                           "\n1 = 20" +
                           "\n2 = 30");
    }
    
    private class TestList
    {
        public List<string> ListProperty { get; set; }
    }

    [Test]
    public void PrintToString_WithList()
    {
        var obj = new TestList
        {
            ListProperty = new List<string> { "One", "Two", "Three" }
        };

        var printer = ObjectPrinter.For<TestList>();
        var result = printer.PrintToString(obj);

        result.Should().Be("TestList:" +
                           "\n0 = One" +
                           "\n1 = Two" +
                           "\n2 = Three");
    }
}