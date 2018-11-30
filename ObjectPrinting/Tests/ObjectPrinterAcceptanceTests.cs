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
            var person = new Person {Name = "Alex", Age = 19};

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<int>()

                //2. Указать альтернативный способ сериализации для определенного типа
                .Serialize<int>().Using(l => l.ToString())

                //3. Для числовых типов указать культуру
                .Serialize<int>().SetCulture(CultureInfo.CurrentCulture)
                
                //4. Настроить сериализацию конкретного свойства
                .Serialize(p => p.Age).Using(a => a.ToString())

                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serialize(p => p.Name).TrimToLength(8)

                //6. Исключить из сериализации конкретного свойства
                .Serialize(p => p.Name).Exclude();
        }
    }
}

