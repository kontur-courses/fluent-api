using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;
using ObjectPrintingShould.ObjectsForTest;

namespace ObjectPrintingShould
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var collections = new Collections
            {
                Arr = new[] {1, 2, 3}, Lis = new List<string> {"dafs", "fkda;", "fdjai"},
                Dict = new Dictionary<int, double> {{1, 2d}, {2, 3d}, {3, 4d}}
            };
            var printerCol = ObjectConfig.For<Collections>().Exclude(item => item.Dict).Build();

            var printer = ObjectConfig.For<Person>()
                //1. Исключить из сериализации свойства определенного типа !
                .Exclude<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                //3. Для числовых типов указать культуру
                .ConfigureType<int>().SetCulture(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .ConfigureProperty(p => p.Name).TrimByLength(2)
                //6. Исключить из сериализации конкретного свойства !
                .Exclude(p => p.Height)
                .Build();

            var s = printerCol.PrintToString(collections);
            Console.WriteLine(s);

            var s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
            
            // //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            // var s2 = person.PrintToString();
            // Console.WriteLine(s2);
            //
            // //8. ...с конфигурированием
            // var s3 = person.PrintToString(c => c.Exclude(p => p.Id).Build());
            // Console.WriteLine(s3);
        }

        public void PrintToString_ClearItem_ItemWithSerialize(Person person, string result)
        {
            var printer = ObjectConfig.For<Person>().Build();
            printer.PrintToString(person).Should().Be(result);
        }
    }
}