using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests;

[TestFixture]
internal class AcceptanceTests : PersonFixture
{
    [Test]
    public void Person_EndToEnd_ShouldHonorFullConfiguration()
    {
        var result = person.PrintToString(cfg => cfg
            .Excluding<Guid>()                                         
            .Printing(p => p.Name).TrimmedToLength(4)                  
            .Printing<double>().Using(d => d.ToString("0.00", CultureInfo.InvariantCulture))
            .Printing(p => p.BirthDate).Using(new CultureInfo("ru-RU"))
            .Excluding(p => p.Address.Street));                       
            
        result.Should()
            .Contain(nameof(Person))
            .And.NotContain(new Guid().ToString())
            .And.Contain("Name = Alex")
            .And.NotContain("Name = Alexandr")
            .And.Contain("Height = 1.85")
            .And.Contain("BirthDate = 12.05.1980")
            .And.Contain("Status = Active")
            .And.Contain("City = Moscow")
            .And.Contain("Building = 7")
            .And.NotContain("Street")
            .And.Contain("[0] = 10")
            .And.Contain("[1] = 20")
            .And.Contain("[2] = 30");
    }

    [Test]
    public void Dictionary_EndToEnd_ShouldPrintKeyValuePairs()
    {
        var obj = new WithDictionary
        {
            Map = new Dictionary<string, int> { ["one"] = 1, ["two"] = 2 }
        };

        obj.PrintToString().Should()
            .Contain(nameof(WithDictionary))
            .And.Contain("[one] = 1")
            .And.Contain("[two] = 2");
    }

    [Test]
    public void Node_EndToEnd_ShouldDetectCycle()
    {
        var a = new Node { Name = "A" };
        var b = new Node { Name = "B", Next = a };
        a.Next = b;

        a.PrintToString().Should()
            .Contain("Name = A")
            .And.Contain("Name = B")
            .And.Contain("cyclic reference to Node");
    }

    [Test]
    public void TwoAddresses_EndToEnd_ShouldNotTreatSharedObjectAsCycle()
    {
        var shared = new Address { City = "Kazan", Street = "Bauman", Building = 1 };
        var obj = new TwoAddresses { First = shared, Second = shared };

        var result = obj.PrintToString();

        result.Should()
            .NotContain("cyclic reference")
            .And.Contain("City = Kazan")
            .And.Contain("Street = Bauman");
        result.Split("City = Kazan").Length.Should().Be(3); 
    }

    [Test]
    public void Person_EndToEnd_WithTypeSerializerOverriddenByMemberSerializer()
    {
        var result = person.PrintToString(cfg => cfg
            .Printing<int>().Using(i => $"type:{i}")
            .Printing(p => p.Age).Using(i => $"member:{i}"));

        result.Should()
            .Contain("Age = member:42")
            .And.Contain("[0] = type:1")
            .And.Contain("Building = type:7");
    }
}