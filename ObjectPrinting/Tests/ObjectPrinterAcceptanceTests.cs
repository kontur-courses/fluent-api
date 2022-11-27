using System;
using System.Globalization;
using System.Threading;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19,Height = 2221.12323, Id = Guid.Empty, Parent = new Person(){Age = 44,Name = "Anna"}};
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<int>()
                
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<string>().Using(i => $"{i}({i.GetType().Name})")
                ////3. Для числовых типов указать культуру
                .Printing<double>().Using(new CultureInfo("en"))
                ////4. Настроить сериализацию конкретного свойства
                ////5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                //.Printing(p => p.Name).TrimmedToLength(10)
                ////6. Исключить из сериализации конкретного свойства
                //.Excluding(p => p.Age)
                ;

            var s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
             

            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Excluding(p => p.Name));
            Console.WriteLine(s1);
    
            Console.WriteLine(s3);
        }

        [Test]
        public void PrintPerson_WhenExludeIntFields()
        {
            
        }
    }
}