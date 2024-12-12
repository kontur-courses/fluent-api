using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using VerifyNUnit;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterAcceptanceTests
{
    private Person _person;
    private PrintingConfig<Person> _config;

    [SetUp]
    public void SetUp()
    {
        _person = new Person
        {
            Id = new Guid(), Name = "Alex", Surname = "Smith",
            Age = 19, Height = 177.4, Persons = new List<Person>()
        };
        _config = ObjectPrinter.For<Person>();
    }

    [Test]
    [Description("Возможность исключения из сериализации свойств определенного типа")]
    public Task TypeExclusion()
    {
        var str = _config
            .ExceptType<string>()
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }

    [Test]
    [Description("Возможность указать альтернативный способ сериализации для определенного типа")]
    public Task TypeSerializationSpecification()
    {
        var str = _config
            .ForType<string>()
            .Use(o => o.ToUpper())
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }

    [Test]
    [Description("Возможность для числовых типов указать культуру")]
    public Task CultureSpecification()
    {
        var str = _config
            .ForType<double>()
            .UseCulture(CultureInfo.InvariantCulture)
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }
    
    [Test]
    [Description("Возможность настройки сериализации конкретного свойства")]
    public Task PropertySerializationSpecification()
    {
        _person.OtherPerson = new Person { Id = new Guid(), Name = "John", Surname = "Doe" };
        
        var str = _config
            .ForProperty(x => x.OtherPerson!)
            .Use(o => o.Name)
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }
    
    [Test]
    [Description("Возможность обрезания строк")]
    public Task StringTrimming()
    {
        var str = _config
            .ForType<string>()
            .UseMaxLength(2)
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }

    [Test]
    [Description("Возможность обрезания конкретных строковых полей")]
    public Task StringPropertyTrimming()
    {
        var str = _config
            .ForProperty(x => x.Surname)
            .UseMaxLength(2)
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }
    
    [Test]
    [Description("Возможность исключения из сериализации конкретного свойства/поля")]
    public Task PropertyExclusion()
    {
        var str = _config
            .ExceptProperty(x => x.Id)
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }
    
    [Test]
    [Description("Обработка циклических ссылок между объектами")]
    public Task CyclicReferenceSupport()
    {
        _person.OtherPerson = _person;
        
        var str = _config
            .WithMaxRecursionDepth(3)
            .PrintToString(_person);
        
        return Verifier.Verify(str);
    }
    
    [Test]
    [Description("Метод-расширение для сериализации по умолчанию")]
    public Task ExtensionMethod_ForDefaultSerialization()
    {
        var str = _person.PrintToString();
        
        return Verifier.Verify(str);
    }
    
    [Test]
    [Description("Метод-расширение, позволяющий проводить настройку сериализации")]
    public Task ExtensionMethod_WithConfigurationSupport()
    {
        var str = _person.PrintToString(
            config => config.ExceptProperty(x => x.Id));
        
        return Verifier.Verify(str);
    }

    [Test]
    [Description("Сериализация массивов")]
    public Task CollectionSupport_Arrays()
    {
        var arr = new double[] {1, 2, 3.14, 4, 5};
        
        var str = arr.PrintToString(
            config => config
                .ForType<double>()
                .UseCulture(CultureInfo.InvariantCulture));
        
        return Verifier.Verify(str);
    }
    
    [Test]
    [Description("Сериализация списков")]
    public Task CollectionSupport_Lists()
    {
        var list = new List<Person>
        {
            _person,
            new Person { Id = new Guid(), Name = "John", Surname = "Doe" }
        };

        var str = list.PrintToString(
            config => config
                .ForType<Person>()
                .Use(o => o.Name));
        
        return Verifier.Verify(str);
    }
    
    [Test]
    [Description("Сериализация словарей")]
    public Task CollectionSupport_Dictionaries()
    {
        var dictionary = new Dictionary<string, double>
        {
            { "a", 1.1 },
            { "b", 2.2 }
        };

        var str = dictionary.PrintToString(
            config => config
                .ForType<double>()
                .UseCulture(CultureInfo.InvariantCulture));
        
        return Verifier.Verify(str);
    }
}