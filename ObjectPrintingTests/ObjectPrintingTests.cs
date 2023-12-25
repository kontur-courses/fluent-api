using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using System.Globalization;

namespace ObjectPrintingTests;

public class ObjectPrintingTests
{
    private readonly Person person = new() { Age = 20, Height = 178.5, Name = "Test" };

    [Test]
    public void PrintToString_SimplePersonClass_ReturnsStringWithAllPorperties()
    {        
        var result = ObjectPrinter.For<Person>().PrintToString(person);
        var expected = "Person\r\n\tId = Guid\r\n\tName = Test\r\n\tHeight = 178,5\r\n\tAge = 20\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_WithExcludeType_ReturnsCorrectString()
    {
        var result = ObjectPrinter
            .For<Person>()
            .Excluding<Guid>()
            .PrintToString(person);
        var expected = "Person\r\n\tName = Test\r\n\tHeight = 178,5\r\n\tAge = 20\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_WithExcludeProperty_ReturnsCorrectString()
    {
        var result = ObjectPrinter
            .For<Person>()
            .Excluding(p => p.Id)
            .PrintToString(person);
        var expected = "Person\r\n\tName = Test\r\n\tHeight = 178,5\r\n\tAge = 20\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_WithCustomSerializeType_ReturnsCorrectString()
    {
        var result = ObjectPrinter
            .For<Person>()
            .For<int>()
            .Using(i => "x")
            .PrintToString(person);
        var expected = "Person\r\n\tId = Guid\r\n\tName = Test\r\n\tHeight = 178,5\r\n\tAge = x\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_WithCustomSerializeProperty_ReturnsCorrectString()
    {
        var result = ObjectPrinter
            .For<Person>()
            .For(p => p.Id)
            .Using(id => "x")
            .PrintToString(person);
        var expected = "Person\r\n\tId = x\r\n\tName = Test\r\n\tHeight = 178,5\r\n\tAge = 20\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_WithCulture_ReturnsCorrectString()
    {
        var result = ObjectPrinter
            .For<Person>()
            .For<double>()
            .Using<double>(new CultureInfo("en-US"))
            .PrintToString(person);
        var expected = "Person\r\n\tId = Guid\r\n\tName = Test\r\n\tHeight = 178.5\r\n\tAge = 20\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_WithTrimmedString_ReturnsCorrectString()
    {
        var result = ObjectPrinter
            .For<Person>()
            .For(p => p.Name)
            .TrimmedToLength(2)
            .PrintToString(person);
        var expected = "Person\r\n\tId = Guid\r\n\tName = Te\r\n\tHeight = 178,5\r\n\tAge = 20\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_WithList_ReturnsCorrectString()
    {
        var person = new PersonWithList() { FriendNames = new List<string> { "1", "2", "3", "4" } };
        var result = ObjectPrinter
            .For<PersonWithList>()
            .PrintToString(person);
        var expected = "PersonWithList\r\n\tFriendNames = \t\t1\r\n\t\t2\r\n\t\t3\r\n\t\t4\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_PersonWithArray_ReturnsCorrectString()
    {
        var person = new PersonWithArray() { FriendNames = new string[] { "name1", "name2", "name3" } };
        var result = ObjectPrinter
            .For<PersonWithArray>()
            .PrintToString(person);
        var expected = "PersonWithArray\r\n\tFriendNames = \t\tname1\r\n\t\tname2\r\n\t\tname3\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_PersonWithDictionary_ReturnsCorrectString()
    {
        var person = new PersonWithDictionary() { Friends = new Dictionary<int, string> 
        {
            { 1, "name1" }, { 2, "name2" }, { 3, "name3" } 
        }};
        var result = ObjectPrinter
            .For<PersonWithDictionary>()
            .PrintToString(person);
        var expected = "PersonWithDictionary\r\n\tFriends = \t\t1 : name1\r\n\t\t2 : name2\r\n\t\t3 : name3\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_CollectionEmpty_ReturnsCorrectString()
    {
        var person = new PersonWithArray() { FriendNames = Array.Empty<string>() };
        var result = ObjectPrinter
            .For<PersonWithArray>()
            .PrintToString(person);
        var expected = "PersonWithArray\r\n\tFriendNames = Empty\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_WithCyclicReference_ReturnsCorrectString()
    {
        var person = new PersonWithReference { Id = 0, Name = "Anton"};
        person.Friend = person;
        var result = ObjectPrinter
            .For<PersonWithReference>()
            .PrintToString(person);
        var expected = "PersonWithReference\r\n\tId = 0\r\n\tName = Anton\r\n\tFriend = this PersonWithReference\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_PersonWithReference_ReturnsCorrectString()
    {
        var person = new PersonWithReference { Id = 0, Name = "Anton" };
        person.Friend = new PersonWithReference { Friend = null, Id = 0, Name = "Andrey"};
        var result = ObjectPrinter
            .For<PersonWithReference>()
            .PrintToString(person);
        var expected = $"PersonWithReference\r\n\tId = 0\r\n\t" +
            $"Name = Anton\r\n\t" +
            $"Friend = PersonWithReference\r\n\t\t" +
            $"Id = 0\r\n\t\t" +
            $"Name = Andrey\r\n\t\tFriend = null\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_PropertyTrimTwice_ReturnsCorrectString()
    {
        var person = new Person { Age = 10, Name = "Anton" };
        var result = ObjectPrinter
            .For<Person>()
            .For(p => p.Name)
            .TrimmedToLength(1)
            .For(p => p.Name)
            .TrimmedToLength(2)
            .PrintToString(person);
        var expected = "Person\r\n\tId = Guid\r\n\tName = An\r\n\tHeight = 0\r\n\tAge = 10\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_TwoTypesSerializingProperty_ReturnsCorrectStringWithLastSerializing()
    {
        var person = new Person { Age = 10, Name = "Anton" };
        var result = ObjectPrinter
            .For<Person>()
            .For(p => p.Age)
            .Using(a => "x")
            .For(p => p.Age)
            .Using(a => "y")
            .PrintToString(person);
        var expected = "Person\r\n\tId = Guid\r\n\tName = Anton\r\n\tHeight = 0\r\n\tAge = y\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_TwoTypesSerializingType_ReturnsCorrectStringWithLastSerializing()
    {
        var person = new Person { Age = 10, Name = "Anton" };
        var result = ObjectPrinter
            .For<Person>()
            .For<int>()
            .Using(i => "x")
            .For<int>()
            .Using(i => "y")
            .PrintToString(person);
        var expected = "Person\r\n\tId = Guid\r\n\tName = Anton\r\n\tHeight = 0\r\n\tAge = y\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_PropertyExcludeTwice_ReturnsCorrectString()
    {
        var person = new Person { Age = 10, Name = "Anton" };
        var result = ObjectPrinter
            .For<Person>()
            .Excluding(p => p.Age)
            .PrintToString(person);
        var expected = "Person\r\n\tId = Guid\r\n\tName = Anton\r\n\tHeight = 0\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_TypeExcludeTwice_ReturnsCorrectString()
    {
        var person = new Person { Age = 10, Name = "Anton" };
        var result = ObjectPrinter
            .For<Person>()
            .Excluding<int>()
            .PrintToString(person);
        var expected = "Person\r\n\tId = Guid\r\n\tName = Anton\r\n\tHeight = 0\r\n";
        result.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void PrintToString_WithField_ReturnsCorrectString()
    {
        var person = new PersonWithField { Age = 10, Name = "Anton" };
        var result = ObjectPrinter
            .For<PersonWithField>()
            .PrintToString(person);
        var expected = "Person\r\n\tId = Guid\r\n\tName = Anton\r\n\tHeight = 0\r\n";
        result.Should().BeEquivalentTo(expected);
    }
}