using System.Globalization;
using ApprovalTests;
using ApprovalTests.Reporters;
using ObjectPrinting;
using ObjectPrinting.Serializer.Configs.Tools;
using ObjectPrintingTests.Domain;

namespace ObjectPrintingTests;

[TestFixture]
[UseReporter(typeof(RiderReporter))]
[ApprovalTests.Namers.UseApprovalSubdirectory("Snapshots")]
public class ObjectPrinterTest
{
    [Test]
    public void ObjectPrinter_PrintToString_ShouldExcludeType()
    {
        var actual = ObjectPrinter.For<Person>()
            .Excluding<int>()
            .PrintToString(PersonFactory.APersonWithoutId());
        Approvals.Verify(actual);
    }

    [Test]
    public void ObjectPrinter_PrintToString_ShouldUseCustomTypeSerializer()
    {
        var actual = ObjectPrinter
            .For<Person>()
            .Printing<DateTime>().Using(i => i.ToLongDateString())
            .PrintToString(PersonFactory.APersonWithoutId());
        Approvals.Verify(actual);
    }
    
    [Test]
    public void ObjectPrinter_PrintToString_ShouldUseCultureForType()
    {
        var actual = ObjectPrinter
            .For<Person>()
            .Printing<double>().WithCulture(CultureInfo.InvariantCulture)
            .PrintToString(PersonFactory.APersonWithDoubleHeight());
        Approvals.Verify(actual);
    }

    [Test]
    public void ObjectPrinter_PrintToString_ShouldTrimStrings()
    {
        var actual = ObjectPrinter
            .For<Person>()
            .Printing(p => p.Name).TrimmedToLength(2)
            .PrintToString(PersonFactory.APersonWithoutId());
        Approvals.Verify(actual);
    }

    [Test]
    public void ObjectPrinter_PrintToString_ShouldExcludeProperty()
    {
        var actual = ObjectPrinter
            .For<Person>()
            .Excluding(p => p.Height)
            .PrintToString(PersonFactory.APersonWithoutId());
        Approvals.Verify(actual);
    }

    [Test]
    public void ObjectPrinter_PrintToString_ShouldStopInfiniteNesting()
    {
        var actual = ObjectPrinter
            .For<Person>()
            .WithMaxNestingLevel(5)
            .Excluding(p => p.Height)
            .PrintToString(PersonFactory.ACyclePerson());
        Approvals.Verify(actual);
    }

    [Test]
    public void ObjectPrinter_PrintToString_ShouldUseCustomPropertySerializer()
    {
        var actual = ObjectPrinter
            .For<Person>()
            .Printing(p => p.Height).Using(_ => "custom")
            .PrintToString(PersonFactory.APersonWithoutId());
        Approvals.Verify(actual);
    }
}