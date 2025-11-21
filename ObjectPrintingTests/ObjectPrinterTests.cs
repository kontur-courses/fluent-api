using System.Globalization;
using System.Text;
using FluentAssertions;
using ObjectPrinting.Solved;
using ObjectPrinting.Solved.Tests;

namespace ObjectPrintingTests;

public class ObjectPrinterTests
{
    private Person person = new();
    private Guid guid = Guid.NewGuid();
    private string newLine = Environment.NewLine;
    [SetUp]
    public void SetUp()
    {
        person = new Person
        {
            Id = guid,
            Name = "Alex",
            Age = 19,
            Birthday = new DateTime(1982, 06, 01),
            FriendsBirthdays = [new DateTime(1991, 03, 12), new DateTime(1985, 09, 19)],
            FriendsNames = ["John", "Amy", "Martin"],
            Height = 190.5,
            Money = 1000.1m,
            Pets = new() { { "Asya", "Cat" }, { "Garry", "Fish" } },
            Parent = new Person
            {
                Name = "Jack"
            }
        };
    }

    [Test]
    public void PrintToString_ShouldPrintExactly_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .PrintToString(person);

        var expected = new StringBuilder();
        expected.AppendLine("Person")
            .AppendLine($"\tId = {person.Id}")
            .AppendLine($"\tName = {person.Name}")
            .AppendLine($"\tHeight = {person.Height}")
            .AppendLine($"\tAge = {person.Age}")
            .AppendLine($"\tMoney = {person.Money}")
            .AppendLine("\tParent = Person")
            .AppendLine($"\t\tId = {person.Parent.Id}"); // возможно стоит добавить больше, либо разнести их на разные тесты

        result.Should().Contain(expected.ToString());
    }
    
    [Test]
    public void PrintToString_ShouldPrintPrimitiveProperties_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .PrintToString(person);

        result.Should().Contain("Name = Alex")
            .And.Contain("Age = 19")
            .And.Contain($"Id = {guid}")
            .And.Contain("Height = 190,5")
            .And.Contain("Money = 1000,1")
            .And.Contain("Birthday = 01.06.1982 0:00:00");
    }

    [Test]
    public void PrintToString_ShouldPrintArray_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .PrintToString(person);

        result.Should().Contain("FriendsBirthdays = DateTime[]")
            .And.Contain("[0] = 12.03.1991 0:00:00")
            .And.Contain("[1] = 19.09.1985 0:00:00");
    }
    
    [Test]
    public void PrintToString_ShouldPrintList_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .PrintToString(person);

        result.Should().Contain("FriendsNames = List")
            .And.Contain("[0] = John")
            .And.Contain("[1] = Amy")
            .And.Contain("[2] = Martin");
    }
    
    [Test]
    public void PrintToString_ShouldPrintDictionary_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .PrintToString(person);

        result.Should().Contain("Pets = Dictionary")
            .And.Contain("Key = Asya")
            .And.Contain("Value = Cat")
            .And.Contain("Key = Garry")
            .And.Contain("Value = Fish");
    }
    
    [Test]
    public void PrintToString_ShouldPrintInstance_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .PrintToString(person);

        result.Should().Contain("Parent = Person")
            .And.Contain("Name = Jack");
    }
    
    [Test]
    public void PrintToString_ShouldExcludeInstanceType_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Excluding<Person>()
            .PrintToString(person);

        result.Should().NotContain("Parent = Person")
            .And.NotContain("Name = Jack");
    }
    
    [Test]
    public void PrintToString_ShouldExcludeInstanceMember_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Excluding(m => m.Parent)
            .PrintToString(person);

        result.Should().NotContain("Parent = Person")
            .And.NotContain("Name = Jack");
    }


    [Test]
    public void PrintToString_ShouldExcludePrimitiveType_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Excluding<DateTime>()
            .PrintToString(person);

        result.Should().NotContain("Birthday = 01.06.1982 0:00:00");
    }

    [Test]
    public void PrintToString_ShouldExcludePrimitiveMember_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Excluding(m => m.Name)
            .PrintToString(person);

        result.Should().NotContain("Name = Alex");
    }

    [Test]
    public void PrintToString_ShouldExcludeArrayType_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Excluding<DateTime[]>()
            .PrintToString(person);

        result.Should().NotContain("FriendsBirthdays = DateTime[]")
            .And.NotContain("[0] = 12.03.1991 0:00:00")
            .And.NotContain("[1] = 19.09.1985 0:00:00");
    }
    
    [Test]
    public void PrintToString_ShouldExcludeArrayMember_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Excluding(m => m.FriendsBirthdays)
            .PrintToString(person);

        result.Should().NotContain("FriendsBirthdays = DateTime[]")
            .And.NotContain("[0] = 12.03.1991 0:00:00")
            .And.NotContain("[1] = 19.09.1985 0:00:00");
    }

    [Test]
    public void PrintToString_ShouldExcludeListType_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Excluding<List<string>>()
            .PrintToString(person);

        result.Should().NotContain("FriendsNames = List")
            .And.NotContain("[0] = John")
            .And.NotContain("[1] = Amy")
            .And.NotContain("[2] = Martin");
    }
    
    [Test]
    public void PrintToString_ShouldExcludeListMember_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Excluding(m => m.FriendsNames)
            .PrintToString(person);

        result.Should().NotContain("FriendsNames = List")
            .And.NotContain("[0] = John")
            .And.NotContain("[1] = Amy")
            .And.NotContain("[2] = Martin");
    }
    
    [Test]
    public void PrintToString_ShouldExcludeDictionaryType_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Excluding<Dictionary<string, string>>()
            .PrintToString(person);

        result.Should().NotContain("Pets = Dictionary")
            .And.NotContain("Key = Asya")
            .And.NotContain("Value = Cat")
            .And.NotContain("Key = Garry")
            .And.NotContain("Value = Fish");
    }
    
    [Test]
    public void PrintToString_ShouldExcludeDictionaryMember_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Excluding(m => m.Pets)
            .PrintToString(person);

        result.Should().NotContain("Pets = Dictionary")
            .And.NotContain("Key = Asya")
            .And.NotContain("Value = Cat")
            .And.NotContain("Key = Garry")
            .And.NotContain("Value = Fish");
    }

    [Test]
    public void PrintToString_ShouldSerializeStringTypeAlternatively_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing<string>().Using(s => s.Replace("A", "a"))
            .PrintToString(person);
        
        result.Should().Contain("Name = alex");
    }
    
    [Test]
    public void PrintToString_ShouldSerializeDateTimeTypeAlternatively_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing<DateTime>().Using(s => s.ToString(CultureInfo.InvariantCulture).Replace("1982", "1337"))
            .PrintToString(person);
        
        result.Should().Contain("Birthday = 06/01/1337 00:00:00");
    }
    
    [Test]
    public void PrintToString_ShouldSerializeDecimalTypeAlternatively_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing<decimal>().Using(s => s.ToString(CultureInfo.InvariantCulture).Replace("0", "9"))
            .PrintToString(person);
        
        result.Should().Contain("Money = 1999.1");
    }
    
    [Test]
    public void PrintToString_ShouldSerializeStringMemberAlternatively_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing(m => m.Name).Using(s => s.Replace("A", "a"))
            .PrintToString(person);
        
        result.Should().Contain("Name = alex");
    }
    
    [Test]
    public void PrintToString_ShouldSerializeDateTimeMemberAlternatively_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing(m => m.Birthday).Using(s => s.ToString(CultureInfo.InvariantCulture).Replace("1982", "1337"))
            .PrintToString(person);
        
        result.Should().Contain("Birthday = 06/01/1337 00:00:00");
    }
    
    [Test]
    public void PrintToString_ShouldSerializeDecimalMemberAlternatively_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing(m => m.Money).Using(s => s.ToString(CultureInfo.InvariantCulture).Replace("0", "9"))
            .PrintToString(person);
        
        result.Should().Contain("Money = 1999.1");
    }

    [Test]
    public void PrintToString_ShouldSerializeArrayTypeAlternatively_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing<DateTime[]>().Using(s => string.Join(" ", s).Replace("1", "2"))
            .PrintToString(person);
        
        result.Should().Contain("FriendsBirthdays = 22.03.2992 0:00:00 29.09.2985 0:00:00");
    }
    
    [Test]
    public void PrintToString_ShouldSerializeListTypeAlternatively_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing<List<string>>().Using(s => string.Join(" ", s).Replace("A", "a"))
            .PrintToString(person);
        
        result.Should().Contain("FriendsNames = John amy Martin");
    }
    
    [Test]
    public void PrintToString_ShouldSerializeDictionaryTypeAlternatively_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing<Dictionary<string, string>>().Using(s => string.Join(" ", s).Replace("1", "2"))
            .PrintToString(person);
        
        result.Should().Contain("Pets = [Asya, Cat] [Garry, Fish]");
    }
    
    [Test]
    public void PrintToString_ShouldSerializeArrayMemberAlternatively_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing(m => m.FriendsBirthdays).Using(s => string.Join(" ", s).Replace("1", "2"))
            .PrintToString(person);
        
        result.Should().Contain("FriendsBirthdays = 22.03.2992 0:00:00 29.09.2985 0:00:00");
    }
    
    [Test]
    public void PrintToString_ShouldSerializeListMemberAlternatively_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing(m => m.FriendsNames).Using(s => string.Join(" ", s).Replace("A", "a"))
            .PrintToString(person);
        
        result.Should().Contain("FriendsNames = John amy Martin");
    }
    
    [Test]
    public void PrintToString_ShouldSerializeDictionaryMemberAlternatively_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing(m => m.Pets).Using(s => string.Join(" ", s).Replace("1", "2"))
            .PrintToString(person);
        
        result.Should().Contain("Pets = [Asya, Cat] [Garry, Fish]");
    }

    [Test]
    public void PrintToString_ShouldSerializeDoubleWithCulture_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing(m => m.Height).Using(CultureInfo.InvariantCulture)
            .PrintToString(person);
        
        result.Should().Contain("Height = 190.5");
        result.Should().Contain("Money = 1000,1");
    }
    
    [Test]
    public void PrintToString_ShouldSerializeDecimalWithCulture_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing(m => m.Money).Using(CultureInfo.InvariantCulture)
            .PrintToString(person);
        
        result.Should().Contain("Money = 1000.1");
        result.Should().Contain("Height = 190,5");
    }

    [Test]
    public void PrintToString_ShouldSerializeStringWithTrimming_WhenAllPropertiesAreSet()
    {
        var result = ObjectPrinter.For<Person>()
            .Printing(m => m.Name).TrimmedToLength(2)
            .PrintToString(person);
        
        result.Should().NotContain("Name = Alex")
            .And.Contain("Name = Al");
    }

    [Test]
    public void PrintToString_ShouldNotSerializeCyclicReference_WhenCyclicReferenceIsPresent()
    {
        var a = new CyclicReference
        {
            Name = "Test"
        };

        var b = new CyclicReference
        {
            Name = "Test2"
        };
        
        a.Obj = b;
        b.Obj = a;

        var result = ObjectPrinter.For<CyclicReference>()
            .PrintToString(a);
        
        result.Should().Contain("Obj = Cyclic reference at CyclicReference");
    }

    [Test]
    public void PrintToString_ShouldNotPrintCyclicReference_WhenTwoReferencesOnClassPresent()
    {
        var a = new TestDto { Id = 1 };
        var b = new TestModel { A = a, B = a };

        var result = ObjectPrinter.For<TestModel>()
            .PrintToString(b);
        
        result.Should().NotContain("Cyclic reference at TestDto");
    }
}