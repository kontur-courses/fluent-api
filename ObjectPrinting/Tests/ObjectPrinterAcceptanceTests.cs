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
            

            var printer = ObjectPrinter.For<Person>()
                    //1. Исключить из сериализации свойства определенного типа
                    //.Exclude<Guid>()
//                //2. Указать альтернативный способ сериализации для определенного типа
                    .Serializing<int>().Using(s => "##" + s.ToString())
//                //3. Для числовых типов указать культуру
                    .Serializing<double>().Using(CultureInfo.CurrentCulture)
//                //4. Настроить сериализацию конкретного свойства
                    .Serializing(p => p.Id).Using(s => s.ToString() + "sdf")
//                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                    .Serializing(p => p.Name).Cut(2)

//                //6. Исключить из сериализации конкретного свойства
                    .Exclude(p => p.Id)
                ;

            var newPrinter = ObjectPrinter.For<int[]>();
            var son = new int[] {1, 5, 0, 6, 10, 19};
            var s1 = newPrinter.PrintToString(son);
            Console.WriteLine(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию   


            //8. ...с конфигурированием
        }
    }
}