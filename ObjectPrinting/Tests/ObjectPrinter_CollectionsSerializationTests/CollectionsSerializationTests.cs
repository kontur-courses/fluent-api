using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using VerifyNUnit;

namespace ObjectPrinting.Tests.ObjectPrinter_CollectionsSerializationTests;

[TestFixture]
public class CollectionsSerializationTests
{
    private Person _person;
    private Person _otherPerson;
    private Person[] _persons;
    private List<Person> _personList;
    
    [SetUp]
    public void SetUp()
    {
        _person = new Person
        {
            Id = new Guid(), Name = "Alex", Surname = "Smith",
            Age = 19, Height = 177.4, Persons = new List<Person>()
        };
        _otherPerson = new Person { Id = new Guid(), Name = "John", Surname = "Doe" };
        
        _persons = new Person[] { _person, _otherPerson };
        
        _personList = new List<Person>();
        _personList.Add(_person);
        _personList.Add(_otherPerson);
    }

    [Test]
    public Task ArrayOfNonValueType()
    {
        var str = ObjectPrinter.For<Person[]>()
            .PrintToString(_persons);
        
        return Verifier.Verify(str);
    }

    [Test]
    public Task ListOfNonValueType()
    {
        var str = ObjectPrinter.For<List<Person>>()
            .PrintToString(_personList);
        
        return Verifier.Verify(str);
    }

    [Test]
    public Task ListOfArrays()
    {
        var list = new List<Person[]>();
        list.Add(new Person[] { _person, _otherPerson });
        list.Add(new Person[] { new Person{ Name = "Veronica" }, new Person { Name = "Steve" } });
        
        var str = ObjectPrinter.For<List<Person[]>>()
            .PrintToString(list);
        
        return Verifier.Verify(str);
    }

    [Test]
    public Task DictionaryOfNonValueType()
    {
        var dictionary = new Dictionary<string, Person>();
        dictionary.Add("John", _otherPerson);
        dictionary.Add("Alex", _person);
        dictionary.Add("Steve", new Person { Name = "Steve", Surname = "Smith" });
        
        var str = ObjectPrinter.For<Dictionary<string, Person>>()
            .PrintToString(dictionary);
        
        return Verifier.Verify(str);
    }
    
    [Test]
    public Task DictionaryOfArrays()
    {
        var dictionary = new Dictionary<string, Person[]>();
        dictionary.Add("one", _persons);
        dictionary.Add("two", new Person[] { new Person { Name = "Steve", Surname = "Doe" } });
        
        var str = ObjectPrinter.For<Dictionary<string, Person[]>>()
            .PrintToString(dictionary);
        
        return Verifier.Verify(str);
    }

    [Test]
    public Task DictionaryWithNonValueTypeKey()
    {
        var dictionary = new Dictionary<Person, Person>();
        dictionary.Add(_person, _otherPerson);
        dictionary.Add(_otherPerson, new Person { Name = "Steve", Surname = "Doe" });
        
        var str = ObjectPrinter.For<Dictionary<Person, Person>>()
            .PrintToString(dictionary);
        
        return Verifier.Verify(str);
    }
}