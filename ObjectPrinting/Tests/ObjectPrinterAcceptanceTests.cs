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
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter
                .For<Person>()
                .Exclude<Guid>()
                .Printing<int>()
                .Using(i => i.ToString("X"))
                .Printing<double>()
                .Using(CultureInfo.InvariantCulture)
                .Printing(p => p.Age)
                .Using(age => age + " years")
                .Printing(p => p.Name)
                .TrimmedToLength(10)
                ;
                //6. Исключить из сериализации конкретного свойства
            
            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }
    }
}