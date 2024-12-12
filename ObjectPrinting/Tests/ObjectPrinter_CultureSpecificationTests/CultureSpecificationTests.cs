using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using NUnit.Framework;
using VerifyNUnit;

namespace ObjectPrinting.Tests.ObjectPrinter_CultureSpecificationTests;

[TestFixture]
public class CultureSpecificationTests
{
    private Person _person;
    private PrintingConfig<Person> _personConfig;
    private DateTime _dateTime;
    private float _float;
    
    [SetUp]
    public void SetUp()
    {
        _person = new Person
        {
            Id = new Guid(), Name = "Alex", Surname = "Smith",
            Age = 19, Height = 177.4, Persons = new List<Person>()
        };
        
        _personConfig = ObjectPrinter.For<Person>();

        _dateTime = DateTime.Parse("2021-09-01");
        _float = 3.14f;
    }

    [Test]
    public Task DoubleCultureSpecification()
    {
        var str = _personConfig
            .ForType<double>()
            .UseCulture(CultureInfo.InvariantCulture)
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }
    
    [Test]
    public Task DefaultDoubleCultureSpecification()
    {
        var str = _personConfig
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }
    
    [Test]
    public Task FloatCultureSpecification()
    {
        var str = ObjectPrinter.For<float>()
            .ForType<float>()
            .UseCulture(CultureInfo.InvariantCulture)
            .PrintToString(_float);
        
        return Verifier.Verify(str);
    }
    
    [Test]
    public Task DefaultFloatCultureSpecification()
    {
        var str = ObjectPrinter.For<float>()
            .PrintToString(_float);
        
        return Verifier.Verify(str);
    }

    [Test]
    public Task DateCultureSpecification()
    {
        var str = ObjectPrinter.For<DateTime>()
            .ForType<DateTime>()
            .UseCulture(CultureInfo.InvariantCulture)
            .PrintToString(_dateTime);
        
        return Verifier.Verify(str);
    }

    [Test]
    public Task DefaultDateCultureSpecification()
    {
        var str = ObjectPrinter.For<DateTime>()
            .PrintToString(_dateTime);
        
        return Verifier.Verify(str);
    }
}