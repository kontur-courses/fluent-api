using System;
using System.Collections.Generic;
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
            var person = new Person() {Name = "Anna", Age = 14, Height = 100};
            
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .SetAltSerialize<Person>().Using(p => p.Age.ToString())
                //3. Для числовых типов указать культуру
                .SetAltSerialize<long>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .SetAltSerialize(p => p.Name).Using(i => i.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .SetAltSerialize(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Age);

            var pers = new List<Person>();
            pers.Add(new Person() {Name = "Anna", Age = 14, Height = 100});
            pers.Add(new Person() {Name = "Ben", Age = 18, Height = 120});
            pers.Add(new Person() {Name = "Zak", Age = 22, Height = 111});


            var collPrinter = ObjectPrinter.For<IEnumerable<Person>>();

            var x = collPrinter.PrintToString(pers);
            Console.WriteLine(x);
            
            
            //string s1 = printer.PrintToString(person);


            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();
            
            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Exclude(p => p.Age));
            
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }
        
        
    }
}