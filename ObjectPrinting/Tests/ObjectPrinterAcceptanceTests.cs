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


            var printer = ObjectPrinter.For<Person>()
//                1. Исключить из сериализации свойства определенного типа
                    .Exclude<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Serializing<int>().Using(s => "##" + s.ToString())
//                //3. Для числовых типов указать культуру
                .Serializing<double>().Using(CultureInfo.CurrentCulture)
//                //4. Настроить сериализацию конкретного свойства
                .Serializing(p => p.Id).Using(s => s.ToString() + "sdf")
//                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serializing(p => p.Name).TrimmedToLength(100)

//                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Id);


            var son = new Person
            {
                Height = 200,
                Age = 20,
                Name = "bosyata",
                Id = new Guid()
            };

        //  Console.WriteLine(son.PrintToString());
            var car = new Car() {Age = 35, Name = "toyota", Numbers = new List<int>{1,5,8,9,11,235}};
            var dic = new Dictionary<int, string>
            {
                { 32, "hello" },
                { 45, "its" },
                { 5, "me" },
                { 2, "i was" },
            };
            var list = new SortedSet<int>
            {
                1, 5, 89, 52, 8, 5, 5, 5, 52, 8, 4, 8974, 9, 52, 3, 6, 7, 4
            };
            var str = "dxfhbjkml,;";
            Console.WriteLine(car.PrintToString());
        }
    }
}