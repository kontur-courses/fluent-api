using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using VerifyNUnit;

namespace ObjectPrinting.Tests.ObjectPrinter_PropertyExclusionTests;

[TestFixture]
public class PropertyExclusionTests
{
    private Person _person;
    private Person _otherPerson;
    private PersonDatabase _personDatabase;
    private PrintingConfig<Person> _personConfig;
    private PrintingConfig<PersonDatabase> _personDatabaseConfig;
    
    [SetUp]
    public void SetUp()
    {
        _person = new Person
        {
            Id = new Guid(), Name = "Alex", Surname = "Smith",
            Age = 19, Height = 177.4, Persons = new List<Person>()
        };
        _otherPerson = new Person { Id = new Guid(), Name = "John", Surname = "Doe" };
        
        _personDatabase = new PersonDatabase();
        _personDatabase.People.Add(_person);
        _personDatabase.People.Add(_otherPerson);
        _personDatabase.Owner = _person;
        
        _personConfig = ObjectPrinter.For<Person>();
        _personDatabaseConfig = ObjectPrinter.For<PersonDatabase>();
    }

    [Test]
    public Task MultipleTypeExclusion()
    {
        var str = _personConfig
            .ExceptProperty(x => x.Id)
            .ExceptProperty(x => x.OtherPerson!)
            .ExceptProperty(x => x.Persons)
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }

    [Test]
    public Task ExcludesTypesOfFieldsOfInnerObjects()
    {
        var str = _personDatabaseConfig
            .ExceptProperty(x => x.Owner.Id)
            .ExceptProperty(x => x.Owner.OtherPerson!)
            .ExceptProperty(x => x.Owner.Persons)
            .PrintToString(_personDatabase);
        
        return Verifier.Verify(str);
    }
}