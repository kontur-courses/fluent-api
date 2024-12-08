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
            var person = new Person
            {
                Name = "Alex",
                Surname = "Kash",
                Age = 19,
                Parent = new Person
                {
                    Name = "Bob",
                    Surname = "Kash",
                    Age = 40,
                    Parent = new Person()
                },
                Friends =
                [
                    new Person
                    {
                        Name = "Alice",
                        Surname = "Molk",
                        Age = 19,
                    },
                    new Person
                    {
                        Name = "Robert",
                        Surname = "Molk",
                        Age = 19,
                    },
                ],
                SomeDictionary = new Dictionary<int, string>
                {
                    { 1, "One" },
                    { 2, "Two" },
                    { 3, "Three" }
                }
            };

            var printer = ObjectPrinter.For<Person>()

                //1. Исключить из сериализации свойства определенного типа
                .Exclude<Guid>()

                //2. Указать альтернативный способ сериализации для определенного типа
                .PrintSettings<int>().Using(i => $"***{i}***")

                //3. Для числовых типов указать культуру
                .UseCulture<double>(CultureInfo.InvariantCulture)

                //4. Настроить сериализацию конкретного свойства
                .PrintSettings(x => x.Name).Using(p => $"---{p}---")

                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .PrintSettings(x => x.Surname).Trim(2)

                //6. Исключить из сериализации конкретного свойства
                .Exclude(x => x.Height);

            var s1 = printer.PrintToString(person);
            Console.WriteLine(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();
            Console.WriteLine(s2);

            //8. ...с конфигурированием
            var s3 = person.PrintToString(p => p.Exclude(x => x.Surname));
            Console.WriteLine(s3);
        }
    }
}