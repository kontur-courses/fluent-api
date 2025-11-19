using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person {Id = Guid.NewGuid(), Name = "Alex", Age = 19, Height = 180};

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .ExcludeType<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .SerializeType<int>().Use(prop => "Int Number: " + prop.ToString())
                //3. Для числовых типов указать культуру
                .SerializeType<double>().Use(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .SerializeProperty(obj => obj.Height).Use(prop => "Person Height: " + prop.ToString()) 
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .SerializeProperty(obj => obj.Name).TrimTo(1) 
                //6. Исключить из сериализации конкретного свойства
                .ExcludeProperty(obj => obj.Age);
            
            var s1 = printer.PrintToString(person);
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();
            //8. ...с конфигурированием
            var s3 = person.PrintToString(config => 
                config.ExcludeType<Guid>()
                    .SerializeProperty(p => p.Name).TrimTo(2));
        }
    }
}