using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ObjectPrinting;
using ObjectPrinting.Tests;

namespace TestPrinter
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
//                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                //.Printing<int>().Using(i => "хуй тебе, а не свойство")
                //3. Для числовых типов указать культуру
                //      .Printing<double>().Using(CultureInfo.InstalledUICulture)
                //4. Настроить сериализацию конкретного свойства
                //.Printing(p => p.Age).Using(i => "такой молодой?");
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                           // .Printing(p => p.Name).TrimmedToLength(2)
                //6. Исключить из сериализации конкретного свойства
                          .Excluding(p => p.Age);
                ;
            string s1 = printer.PrintToString(person);
            
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
        //    string s2 = person.PrintToString();
            
            //8. ...с конфигурированием
          //  string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            //Console.WriteLine(s2);
            //Console.WriteLine(s3);

            
            Console.ReadLine();
        }
    }
}