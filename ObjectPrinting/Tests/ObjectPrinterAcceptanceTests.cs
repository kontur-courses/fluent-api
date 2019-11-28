using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        /*
         * Разработать библиотеку для преобразования любого объекта в строку, перечисляя значения публичных свойств и полей объекта.
            Все должно гибко настраиваться. А именно:

             1. Исключить из сериализации свойства определенного типа
             2. Указать альтернативный способ сериализации для определенного типа
             3. Для числовых типов указать культуру
             4. Настроить сериализацию конкретного свойства
             5. Настроить обрезание значений строковых свойств
             6. Исключить из сериализации конкретное свойство

            Также от решения ожидается:
             1. Поддержка коллекций (массивы, списки, словари*, ...)
             2. Корректная обработка циклических ссылок между объектами (не должны приводить к `StackOverflowException`)
         */
        [Test]
        public void Demo()
        {
            var person = new Person {Name = "Alex", Age = 19};
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Age).Using(i => i.ToString("D5"))
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(10)
                //6. Исключить из сериализации конкретное свойство
                .Excluding(p => p.Age);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();
            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }
    }
}