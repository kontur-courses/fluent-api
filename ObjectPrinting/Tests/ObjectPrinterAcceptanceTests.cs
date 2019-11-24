using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void WorksWithoutAnyChain()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>();
                //.Printing<string>().Using(x => String.Format("{0} : {1}", x, x.Length));
//                .WithSerilizing<int>().Using(CultureInfo.CurrentCulture)
//                .WithSerilizing(x => x.Name).Using<string>(x => String.Format("{0} : {1}", x, x.Length))
//                .WithSerilizing(x => x.Name).Trim(5)
//                .Excluding(x => x.Name);
                
                //.Serializing<DateTime>().Using(d => d.toString());
                // Serializing(x => x.Name).Using();
                ;
            //1. Исключить из сериализации свойства определенного типа
                //2. Указать альтернативный способ сериализации для определенного типа
                //3. Для числовых типов указать культуру
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                //6. Исключить из сериализации конкретного свойства
            
            string s1 = printer.PrintToString(person);
            
            Console.WriteLine(s1);
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }
    }
}