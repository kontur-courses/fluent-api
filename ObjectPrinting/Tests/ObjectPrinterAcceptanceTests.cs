using System;
using System.Globalization;
using System.IO;
using FluentAssertions;
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

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(k => k.ToString()+" special serrialize")
                //3. Для числовых типов указать культуру
                .Printing<int>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Name).Using(s => s.Trim())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimToLength(5)
                //6. Исключить из сериализации конкретного свойства
                .Exluding(p => p.Name);

            printer = ObjectPrinter.For<Person>().Exluding<Guid>().Printing<int>().Using(k => k.ToString() + " special serrialize");
            string s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
            var path = Path.Combine(Path.GetTempPath(), "test.txt");
            using (StreamWriter sw = new StreamWriter(path, false))
            {
                sw.Write(s1);
            }
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }
    }
}