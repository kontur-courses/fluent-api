using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using VerifyNUnit;

namespace ObjectPrinting.Tests.ObjectPrinter_TypeSerializationTests;

[TestFixture]
public class TypeSerializationTests
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
    public Task BasicTypeSerializationSpecificationTest()
    {
        var str = _personConfig
            .ForType<string>()
            .Use(o => o.ToUpper())
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }
    
    [Test]
    public Task MultipleTypeSerializationSpecificationTest()
    {
        var str = _personConfig
            .ForType<string>()
            .Use(o => o.ToUpper())
            .ForType<Guid>()
            .Use(o => o.ToString().Substring(0, 5))
            .ForType<int>()
            .Use(o => (-o).ToString())
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }
    
    [Test]
    public Task TypeSerializationSpecificationForComplexObjects()
    {
        var str = _personDatabaseConfig
            .ForType<Person>()
            .Use(o => o.Name)
            .PrintToString(_personDatabase);
        
        return Verifier.Verify(str);
    }
}