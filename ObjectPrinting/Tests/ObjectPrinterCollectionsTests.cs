using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterCollectionsTests
{
    [Test]
    public void PrintToString_DictionaryTest()
    {
        var dictionary = new Dictionary<int, Person>()
        {
            { 1, new Person() { Name = "Alex1" } },
            { 2, new Person() { Age = 22 } },
            { 3, new Person() { Height = 12.5 } }
        };

        var expected = "Dictionary`2\r\n\t{\r\n\tKeyValuePair`2\r\n\t\tKey = 1\r\n\t\tValue = Person\r\n\t\t\tId = Guid\r\n\t\t\tHeight = 0\r\n\t\t\tAge = 0\r\n\t\t\tName = Alex1\r\n\t\t\tParent = null\r\n\tKeyValuePair`2\r\n\t\tKey = 2\r\n\t\tValue = Person\r\n\t\t\tId = Guid\r\n\t\t\tHeight = 0\r\n\t\t\tAge = 22\r\n\t\t\tName = null\r\n\t\t\tParent = null\r\n\tKeyValuePair`2\r\n\t\tKey = 3\r\n\t\tValue = Person\r\n\t\t\tId = Guid\r\n\t\t\tHeight = 12,5\r\n\t\t\tAge = 0\r\n\t\t\tName = null\r\n\t\t\tParent = null\r\n\t}\r\n\tComparer = GenericEqualityComparer`1\r\n\tCount = 3\r\n\tKeys = KeyCollection\r\n\t\t{\r\n\t\t1\r\n\t\t2\r\n\t\t3\r\n\t\t}\r\n\t\tCount = 3\r\n\tValues = ValueCollection\r\n\t\t{\r\n\t\tPerson\r\n\t\tPerson\r\n\t\tPerson\r\n\t\t}\r\n\t\tCount = 3\r\n";
        var actual = ObjectExtensions.ToString(dictionary);
        Console.WriteLine(actual);
        
        actual.Should().Be(expected);
    }
        
    [Test]
    public void PrintToString_ListTest()
    {
        var collection = new List<Person>()
        {
            new() { Name = "Alex1" }, 
            new() { Age = 22 }, 
            new() { Height = 12.5 }
        };
        var expected = "List`1\r\n\t{\r\n\tPerson\r\n\t\tId = Guid\r\n\t\tHeight = 0\r\n\t\tAge = 0\r\n\t\tName = Alex1\r\n\t\tParent = null\r\n\tPerson\r\n\t\tId = Guid\r\n\t\tHeight = 0\r\n\t\tAge = 22\r\n\t\tName = null\r\n\t\tParent = null\r\n\tPerson\r\n\t\tId = Guid\r\n\t\tHeight = 12,5\r\n\t\tAge = 0\r\n\t\tName = null\r\n\t\tParent = null\r\n\t}\r\n\tCapacity = 4\r\n\tCount = 3\r\n";
        
        var actual = ObjectExtensions.ToString(collection);
        Console.WriteLine(actual);
        
        actual.Should().Be(expected);
    }
        
    [Test]
    public void PrintToString_ArrayTest()
    {
        var collection = new Person[]
        {
            new() { Name = "Alex1" }, 
            new() { Age = 22 }, 
            new() { Height = 12.5 }
        };
        var expected = "Person[]\r\n\t{\r\n\tPerson\r\n\t\tId = Guid\r\n\t\tHeight = 0\r\n\t\tAge = 0\r\n\t\tName = Alex1\r\n\t\tParent = null\r\n\tPerson\r\n\t\tId = Guid\r\n\t\tHeight = 0\r\n\t\tAge = 22\r\n\t\tName = null\r\n\t\tParent = null\r\n\tPerson\r\n\t\tId = Guid\r\n\t\tHeight = 12,5\r\n\t\tAge = 0\r\n\t\tName = null\r\n\t\tParent = null\r\n\t}\r\n\tLength = 3\r\n\tLongLength = Int64\r\n\tRank = 1\r\n\tIsReadOnly = Boolean\r\n\tIsFixedSize = Boolean\r\n\tIsSynchronized = Boolean\r\n\tSyncRoot = Person[]\r\n\t\t{\r\n\t\tPerson\r\n\t\tPerson\r\n\t\tPerson\r\n\t\t}\r\n";
        
        var actual = ObjectExtensions.ToString(collection);
        Console.WriteLine(actual);
        
        actual.Should().Be(expected);
    }
}