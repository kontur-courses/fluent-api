using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using VerifyNUnit;

namespace ObjectPrinting.Tests.ObjectPrinter_StringTrimmingTests;

[TestFixture]
public class StringTrimmingTests
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
        _person.OtherPerson = _otherPerson;
        
        _personConfig = ObjectPrinter.For<Person>();
    }

    [Test]
    public Task StringPropertiesAreTrimmedCorrectly()
    {
        var str = _personConfig
            .ForType<string>()
            .UseMaxLength(2)
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }

    [Test]
    public Task StringPropertiesBecomeEmptyWhenLengthIsZero()
    {
        var str = _personConfig
            .ForType<string>()
            .UseMaxLength(0)
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }
    
    [Test]
    public Task StringPropertiesDontChangeIfLengthIsBigger()
    {
        var str = _personConfig
            .ForType<string>()
            .UseMaxLength(15)
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }
    
    [Test]
    public Task StringPropertiesDontChangeWithDefaultSettings()
    {
        _person.Name = new string('a', 100);
        
        var str = _personConfig
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }
    
    [Test]
    public void UseMaxLengthThrowsWhenArgumentIsLessThanZero()
    {
        _person.Name = new string('a', 100);

        Action act = () => _personConfig
            .ForType<string>()
            .UseMaxLength(-1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}