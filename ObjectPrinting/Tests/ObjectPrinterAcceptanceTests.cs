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
            var person = GetDefaultPerson();
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age)
                // 9*. Настройка максимального уровня вложенности при сериализации (По умолчанию 10)
                .WithMaxNestingLevel(15);
            
            var s1 = printer.PrintToString(person);
            Console.WriteLine(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию 
            var s2 = person.PrintToString();
            Console.WriteLine(s2);
            
            //8. ...с конфигурированием
            var s3 = person.PrintToString(config => config.Excluding<string>());
            Console.WriteLine(s3);
        }

        private Person GetDefaultPerson()
        {
            return new Person
            {
                Name = "Alex", Age = 19,
                Children = new List<Person>
                {
                    new Person{Name = "c1"},
                    new Person{Name = "c2"}
                },
                Sizes = new[]{1,2,3,4,5,6},
                Relatives = new Dictionary<string, Person>
                {
                    {"father", new Person{Name = "father Name"}},
                    {"mather", new Person{Name = "mather name"}}
                },
                Companies = new Dictionary<string, int>
                {
                    {"youTube", 3},
                    {"fabric", 15},
                    {"railway station", 6}
                }
            };
        }
    }
}