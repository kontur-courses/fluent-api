using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using VerifyNUnit;

namespace ObjectPrinting.Tests.ObjectPrinter_PropertySerializationTests;

[TestFixture]
public class PropertySerializationTests
{
    private Person _person;
    private Person _otherPerson;
    private PrintingConfig<Person> _personConfig;
    
    [SetUp]
    public void SetUp()
    {
        _person = new Person
        {
            Id = new Guid(), Name = "Alex", Surname = "Smith",
            Age = 19, Height = 177.4, Persons = new List<Person>()
        };
        _otherPerson = new Person { Id = new Guid(), Name = "John", Surname = "Doe" };
        
        _personConfig = ObjectPrinter.For<Person>();
    }

    [Test]
    public Task BasicPropertySerializationSpecification()
    {
        var str = _personConfig
            .ForProperty(x => x.Name)
            .Use(o => o.ToUpper())
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }
    
    [Test]
    public Task MultiplePropertySerializationSpecification()
    {
        _person.OtherPerson = _otherPerson;
        
        var str = _personConfig
            .ForProperty(x => x.Name)
            .Use(o => o.ToUpper())
            .ForProperty(x => x.Surname)
            .Use(o => o.ToLower())
            .ForProperty(x => x.OtherPerson)
            .Use(o => o.Id.ToString())
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }

    [Test]
    public Task InnerPropertySerializationSpecification()
    {
        _person.OtherPerson = _otherPerson;

        var str = _personConfig
            .ForProperty(x => x.Name)
            .Use(o => o.ToUpper())
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }
}