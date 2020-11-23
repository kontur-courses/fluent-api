using System.Globalization;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Configuration;

namespace Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person {Name = "Alex", Age = 19};

            ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Choose<int>().Exclude()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Choose<double>().UseSerializer(d => $"My beautiful shiny double: {d}")
                //3. Для числовых типов указать культуру
                .Choose(o => o.Age).SetCulture(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Choose(o => o.Id).UseSerializer(i => i.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Choose(o => o.Name).Trim(10)
                //6. Исключить из сериализации конкретного свойства
                .Choose(o => o.Height).Exclude()
                .PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            person.Serialize();
            //8. ...с конфигурированием
            new object().Serialize(d => d.Choose<int>().Exclude());
            //Ну после такого можно и бутылочку пива бахнуть
            //Я бы ебнул так то
        }
    }
}