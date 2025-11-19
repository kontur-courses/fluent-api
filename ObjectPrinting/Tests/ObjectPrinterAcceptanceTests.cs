using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 19.8};

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .ExcludeType<string>()
                // //2. Указать альтернативный способ сериализации для определенного типа
                //.Configure<int>().Print(x => ...)
                .ConfigureType<int>().WithSerialization(i => i + ".000")
                // //3. Для числовых типов указать культуру
                .ConfigureType<double>().WithCulture(new CultureInfo("en-US"))
                // //4. Настроить сериализацию конкретного свойства 
                .ConfigureProperty(p => p.Age).WithSerialization(age => $"age: {age}")
                // //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .ConfigureProperty(p => p.Name).Trim(0, 5)
                // //6. Исключить из сериализации конкретного свойства
                .ExcludeProperty(p => p.Height)
                ;
            
            string s1 = printer.PrintToString(person);

            // 7. Синтаксический сахар - сериализация по-умолчанию
            string defaultResult = person.PrintToString();
            
            // 8. ...с конфигурированием - компактный синтаксис
            string configuredResult = person.PrintToString(config => config
                .ExcludeType<string>()
                .ConfigureType<int>().WithSerialization(i => i + ".000")
                .ConfigureType<double>().WithCulture(new CultureInfo("en-US"))
                .ConfigureProperty(p => p.Age).WithSerialization(age => $"age: {age}")
                .ConfigureProperty(p => p.Name).Trim(0, 5)
                .ExcludeProperty(p => p.Height)
            );
            
            
        }
    }
}