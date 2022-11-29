using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using ObjectPrinting.Solved;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person
            {
                Name = "Bill",
                Age = 35,
                ArmsLenght = new int[] { 540, 600 },
                FriendsNames = new List<string>(){ "Rick", "Morgan", "Alex" },
                Awards = new Dictionary<string, int>()
                {
                    ["Best friend"] = 12,
                    ["Best dad"] = 25,
                    ["Best worker"] = 29
                }
            };

            var printer = ObjectPrinter.For<Person>();
            var standartConfig = printer.PrintToString(person);

            File.WriteAllText(@"..\\text.txt", standartConfig);

            printer
            //1. Исключить из сериализации свойства определенного типа.
                .SelectType<int>().IgnoreType()
                .SelectType<string>().IgnoreType()
                .SelectType<char>().IgnoreType()
            //2. Указать альтернативный способ сериализации для определенного типа
                .SelectType<string>().PrintAs(x => $"№{x}")
            //3. Для числовых типов указать культуру
                .SelectType<int>().SetCulture(CultureInfo.InvariantCulture)
            //4. Настроить сериализацию конкретного свойства                
                .SelectProperty(x => x.Age).PrintAs(x => $"№{x}")
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .SelectProperty(x => x.Name).TrimmedToLength(10)
            //6. Исключить из сериализации конкретного свойства
                .SelectProperty(x => x.Name).IgnoreProperty();


            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            var configWithExt = person.PrintToString();

            //8. ...с конфигурированием
            var configLine = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(standartConfig);
            Console.WriteLine(configWithExt);
            Console.WriteLine(configLine);

        }
    }
}