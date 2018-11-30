using System;
using System.Globalization;
using ObjectPrinting.Tests;

namespace ObjectPrinting
{
    public class Program
    {
        public static void Main()
        {
            var person = new Person { Name = "Alex", Age = 20 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString())
                //3. Для числовых типов указать культуру
                .Printing<int>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(2)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);
            
            string s1 = printer.PrintToString(person);
            string s2 = person.PrintToString();

            Console.WriteLine(s1);
            Console.WriteLine(s2);
        }
    }
}