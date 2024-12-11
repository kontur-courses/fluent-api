using System.Globalization;
using ObjectPrinting;

namespace ObjectPrintingTests;

public class Tests
{
    [Test]
    public void ObjectPrinterAcceptanceTest()
    {
        var person = new Person(new Guid(), "Alex", 189.25, 19);

        var printer = ObjectPrinter.For<Person>()
            //1. Исключить из сериализации свойства определенного типа
            .Exclude<Guid>()
            //2. Указать альтернативный способ сериализации для определенного типа
            .SetPrintingFor<int>().Using(number => $"Integer - {number}")
            //3. Для числовых типов указать культуру
            .SetPrintingFor<double>().WithCulture(CultureInfo.InvariantCulture)
            //4. Настроить сериализацию конкретного свойства
            .SetPrintingFor(p => p.Name).Using(_ => "Property printing")
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            .SetPrintingFor(p => p.Name).TrimmedToLength(10)
            //6. Исключить из сериализации конкретного свойства
            .Exclude(p => p.Id);
        var s1 = printer.PrintToString(person);
        Console.WriteLine(s1);
        
        //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
        var s2 = person.PrintToString();
        Console.WriteLine(s2);
        
        //8. ...с конфигурированием
        var s3 = person.PrintToString(
            c => c
                .Exclude<int>()
                .Exclude(p => p.Friends)
                .Exclude(p => p.Friend)
                .Exclude(p => p.Relatives)
                .Exclude(p => p.Neighbours)
                .SetSerializationDepth(1)
            );
        Console.WriteLine(s3);

        //9. Для коллекций также работает, при этом Exclude и Using работает для всех элементов коллекций
        var s4 = new List<Person>{ new(new Guid(), "Alex", 189.25, 80), new()}
            .PrintToString(c => c.Exclude<double>());
        Console.WriteLine(s4);
        
        var s5 = new[] { new Person(new Guid(), "Alex", 189.25, 80), new Person() }
            .PrintToString(c => c.Exclude<Guid>());
        Console.WriteLine(s5);
        
        var s6 = new Dictionary<int, Person>{ 
            { 12, new Person(new Guid(), "Alex", 189.25, 80) }, 
            { 19, new Person() } }
            .PrintToString(
                c => c.SetPrintingFor<double>().Using(p => $"--{p}--"));
        Console.WriteLine(s6);
    }
}