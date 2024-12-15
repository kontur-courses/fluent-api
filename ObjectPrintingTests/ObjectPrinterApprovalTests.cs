using System.Globalization;
using ApprovalTests;
using ApprovalTests.Reporters;
using ObjectPrinting;

namespace ObjectPrintingTests;

[UseReporter(typeof(RiderReporter))]
[ApprovalTests.Namers.UseApprovalSubdirectory("Snapshots")]
public class ObjectPrinterApprovalTests
{
    private readonly Person basicPerson = new();
    
    [Test]
    public void PrintToString_ShouldExcludeType()
    {
        var actual = ObjectPrinter
            .For<Person>()
            .Exclude<int>()
            .PrintToString(basicPerson);
        Approvals.Verify(actual);
    }

    [Test]
    public void PrintToString_ShouldUseCustomTypeSerializer()
    {
        var actual = ObjectPrinter
            .For<Person>()
            .SetPrintingFor<double>().Using(number => $"DOUBLE - {number}")
            .PrintToString(basicPerson);
        Approvals.Verify(actual);
    }

    [Test]
    public void PrintToString_ShouldUseCultureForType()
    {
        var actual = ObjectPrinter
            .For<Person>()
            .SetPrintingFor<double>().WithCulture(CultureInfo.InvariantCulture)
            .PrintToString(basicPerson);
        Approvals.Verify(actual);
    }

    [Test]
    public void PrintToString_ShouldTrimStrings()
    {
        var actual = ObjectPrinter
            .For<Person>()
            .SetPrintingFor(p => p.Name).TrimmedToLength(2)
            .PrintToString(basicPerson);
        Approvals.Verify(actual);
    }

    [Test]
    public void PrintToString_ShouldExcludeProperty()
    {
        var actual = ObjectPrinter
            .For<Person>()
            .Exclude(x => x.Name)
            .PrintToString(basicPerson);
        Approvals.Verify(actual);
    }

    [Test]
    public void PrintToString_ShouldStopInfiniteNesting()
    {
        var cycledPerson = new Person();
        cycledPerson.Friend = cycledPerson;
        var actual = ObjectPrinter
            .For<Person>()
            .PrintToString(cycledPerson);
        Approvals.Verify(actual);
    }

    [Test]
    public void PrintToString_ShouldUseCustomPropertySerializer()
    {
        var actual = ObjectPrinter
            .For<Person>()
            .SetPrintingFor(p => p.Name).Using(name => $"Their Name is {name}")
            .PrintToString(basicPerson);
        Approvals.Verify(actual);
    }

    [Test]
    public void PrintToString_ShouldSerializeArray()
    {
        var personWithRelatives = new Person
        {
            Relatives = [basicPerson, basicPerson]
        };
        var actual = ObjectPrinter
            .For<Person>()
            .PrintToString(personWithRelatives);
        Approvals.Verify(actual);
    }
    
    [Test]
    public void PrintToString_ShouldSerializeList()
    {
        var personWithFriends = new Person
        {
            Friends = [basicPerson, basicPerson]
        };
        var actual = ObjectPrinter
            .For<Person>()
            .PrintToString(personWithFriends);
        Approvals.Verify(actual);
    }
    
    [Test]
    public void PrintToString_ShouldSerializeDictionary()
    {
        var personWithRelatives = new Person
        {
            Neighbours = new Dictionary<int, Person>{{ 12, new Person() }, { 19, new Person() } }
        };
        var actual = ObjectPrinter
            .For<Person>()
            .PrintToString(personWithRelatives);
        Approvals.Verify(actual);
    }
}