using System.Globalization;
using ObjectPrinterTests.Entities;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrinterTests;

[TestFixture]
public class ObjectPrinterTests
{
    private static readonly VerifySettings SnapshotSettings;

    static ObjectPrinterTests()
    {
        SnapshotSettings = new VerifySettings();
        SnapshotSettings.UseDirectory("ExpectedResults");
    }
    
    [Test]
    public Task PrintToString_ShouldHandleNullObject_Correctly()
    {
        A? obj = null;

        var result = ObjectPrinter.PrintToString<A>(obj);

        return Verify(result, SnapshotSettings);
    }
    
    [Test]
    public Task PrintToString_ShouldExcludeType_Correctly()
    {
        var obj = new A
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Alex"
        };

        var printer = ObjectPrinter.For<A>()
            .Excluding<Guid>();

        var result = printer.PrintToString(obj);

        return Verify(result, SnapshotSettings);
    }

    [Test]
    public Task PrintToString_ShouldUseCustomSerialization_Correctly()
    {
        var obj = new A { Number = 42 };

        var printer = ObjectPrinter.For<A>()
            .Printing<int>()
            .Using(x => $"INT({x})");

        var result = printer.PrintToString(obj);

        return Verify(result, SnapshotSettings);
    }

    [Test]
    public Task PrintToString_ShouldApplyCultureInfo_Correctly()
    {
        var obj = new A { Price = 1234.56 };

        var printer = ObjectPrinter.For<A>()
            .Printing<double>()
            .Using(CultureInfo.GetCultureInfo("fr-FR"));

        var result = printer.PrintToString(obj);

        return Verify(result, SnapshotSettings);
    }

    [Test]
    public Task PrintToString_ShouldApplyCustomSerializationForProperty_Correctly()
    {
        var obj = new A { Name = "Alex" };

        var printer = ObjectPrinter.For<A>()
            .Printing(a => a.Name)
            .Using(n => $"NAME={n}");

        var result = printer.PrintToString(obj);

        return Verify(result, SnapshotSettings);
    }

    [Test]
    public Task PrintToString_ShouldTrimString_Correctly()
    {
        var obj = new A { Name = "Alexander" };

        var printer = ObjectPrinter.For<A>()
            .Printing(a => a.Name)
            .TrimmedToLength(4);

        var result = printer.PrintToString(obj);

        return Verify(result, SnapshotSettings);
    }

    [Test]
    public Task PrintToString_ShouldExcludeSpecificProperty_Correctly()
    {
        var obj = new A { Name = "Alex", Number = 10 };

        var printer = ObjectPrinter.For<A>()
            .Excluding(a => a.Number);

        var result = printer.PrintToString(obj);

        return Verify(result, SnapshotSettings);
    }

    [Test]
    public Task PrintToString_ShouldHandleCyclicReferences_Correctly()
    {
        var parent = new B();
        var child = new B { Parent = parent };
        parent.Data = new A { Name = "Child" };
        parent.Parent = child;

        var printer = ObjectPrinter.For<B>();

        var result = printer.PrintToString(parent);
        
        return Verify(result, SnapshotSettings);
    }
}
