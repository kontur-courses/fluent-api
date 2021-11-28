using System;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests
{
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person
            {
                Name = "Alex", 
                Surname = "Ivanov", 
                Age = 19, 
                Height = 185,
                Money = 300
            };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства/поля определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<double>().Using(d => $"{d} dollars")
                //3. Для числовых типов указать культуру
                .Printing<int>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Age).Using(x => $"{x} years")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Surname).TrimmedToLength(4)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Height);
            
            string s1 = printer.PrintToString(person);
            Console.WriteLine(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию      
            person.PrintToString();
        }
    }
}