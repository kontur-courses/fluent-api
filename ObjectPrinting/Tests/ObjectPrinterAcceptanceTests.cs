using System;
using System.Linq.Expressions;
using NUnit.Framework;

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
                Father = new Person
                {
                    Mother = new Person
                    {
                        Mother = new Person
                        {
                            Name = "Adam",
                            Father = new Person
                            {
                                Age = 2000,
                                Name = "???"
                            }
                        },
                    }
                },
                Name = "Alex", Age = 19
            };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<string>().As(s => $"<<{s}>>")
                .Printing<DateTime>().As(d => $"year: {d.Year}, the rest is mistery...")
                //3. Для числовых типов указать культуру
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Father.Mother.Mother.Father.Name).As(name => $"who is that?{name}")
                .Printing(p => p.Father.Mother).Using(config => config
                    .Printing<DateTime>().As(d => $"...but there is less mystery: {d}"))
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Mother)
                .Excluding(p => p.Father.Father);
            
            var s1 = printer.PrintToString(person);
            Console.WriteLine(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }
    }
}