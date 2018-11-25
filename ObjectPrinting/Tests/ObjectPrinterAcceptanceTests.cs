using System;
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
                .Exclude<Guid>()
//                //2. Указать альтернативный способ сериализации для определенного типа
                .Serializing<string>().Using(s => "##" + s.ToString());
//                //3. Для числовых типов указать культуру
//                .Serializing<int>().Using(CultureInfo.CurrentCulture)
//                //4. Настроить сериализацию конкретного свойства
//                .Serializing(p => p.Id).Using(s => s.ToString())
//                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
//                .Serializing(p => p.Name).CutLast(100)
//                //6. Исключить из сериализации конкретного свойства
//                .Exclude(p => p.Age);

            var s1 = printer.PrintToString(person);
            Console.WriteLine(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию   

            //8. ...с конфигурированием
        }
    }
}