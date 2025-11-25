using ObjectPrinterTests.Entities;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrinterTests;

[TestFixture]
public class CollectionPrintingSnapshotTests
{
    private static readonly VerifySettings SnapshotSettings;
    
    static CollectionPrintingSnapshotTests()
    {
        SnapshotSettings = new VerifySettings();
        SnapshotSettings.UseDirectory("ExpectedResults");
    }
    
    [Test]
    public Task IntArray_Snapshot()
    {
        var config = new PrintingConfig<int[]>();
        var value = new[] { 1, 2, 3 };

        var actual = config.PrintToString(value);

        return Verify(actual, SnapshotSettings);
    }

    [Test]
    public Task PrintToString_StringList_ShouldHandleCorrect()
    {
        var config = new PrintingConfig<List<string>>();
        var value = new List<string> { "one", "two" };

        var actual = config.PrintToString(value);

        return Verify(actual, SnapshotSettings);
    }

    [Test]
    public Task PrintToString_Dictionary_ShouldHandleCorrect()
    {
        var config = new PrintingConfig<Dictionary<string, int>>();
        var value = new Dictionary<string, int>
        {
            ["a"] = 1,
            ["b"] = 2
        };

        var actual = config.PrintToString(value);

        return Verify(actual, SnapshotSettings);
    }

    [Test]
    public Task PrintToString_ShouldHandleClassWithCollections_Correctly()
    {
        var config = new PrintingConfig<Person>();
        var person = new Person
        {
            Name = "John",
            Scores = [10, 20],
            Tags = ["dev", "qa"],
            Addresses = new Dictionary<string, Address>
            {
                ["home"] = new() { City = "Chelyabinsk" },
                ["work"] = new() { City = "Sverdlovsk" }
            }
        };

        var actual = config.PrintToString(person);

        return Verify(actual, SnapshotSettings);
    }

    [Test]
    public Task PrintToString_ShouldHandleListOfLists_Correctly()
    {
        var config = new PrintingConfig<List<List<int>>>();
        var value = new List<List<int>>
        {
            new() { 1, 2 },
            new() { 3 }
        };

        var actual = config.PrintToString(value);

        return Verify(actual, SnapshotSettings);
    }

    [Test]
    public Task PrintToString_ShouldHandleDictionaryWithListValues_Correctly()
    {
        var config = new PrintingConfig<Dictionary<string, List<int>>>();
        var value = new Dictionary<string, List<int>>
        {
            ["first"] = [1, 2],
            ["second"] = [3]
        };

        var actual = config.PrintToString(value);

        return Verify(actual, SnapshotSettings);
    }
    
    [Test]
    public Task PrintToString_ShouldHandleDictionaryWithNull_Correctly()
    {
        var config = new PrintingConfig<Dictionary<string, List<int>>>();
        var value = new Dictionary<string, List<int>?>
        {
            ["first"] = null,
            ["second"] = null
        };

        var actual = config.PrintToString(value);

        return Verify(actual, SnapshotSettings);
    }
    
    [Test]
    public Task PrintToString_ShouldHandleListWithNull_Correctly()
    {
        var config = new PrintingConfig<Dictionary<string, List<int>>>();
        var value = new List<string?>()
        {
            null,
            null,
            null
        };

        var actual = config.PrintToString(value);

        return Verify(actual, SnapshotSettings);
    }

    [Test]
    public Task PrintToString_ShouldHandleListCycle_Correctly()
    {
        var config = new PrintingConfig<List<object>>();
        var value = new List<object>();
        value.Add(value);

        var actual = config.PrintToString(value);

        return Verify(actual, SnapshotSettings);
    }
}
