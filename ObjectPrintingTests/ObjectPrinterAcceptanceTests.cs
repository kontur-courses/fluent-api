using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrintingTests.Auxiliary;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 10.10};

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => "int")
                //3. Для числовых типов указать культуру
                .Printing<double>().WithCultureInfo(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Age).Using(a => "age").TrimmedToLength(12)
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(2);
                //6. Исключить из сериализации конкретного свойства


            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string s2 = person.PrintToString();

            //8. ...с конфигурированием
            string s3 = person.PrintToString(s => s.Excluding(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        [Test]
        public void DemoNew()
        {
            var wrapper = new Wrapper<Dictionary<string, int>>(
                new Dictionary<string, int>
                    { {"a", 1}, {"b", 2} }
                );

            var result = ObjectPrinter.For<Wrapper<Dictionary<string, int>>>().PrintToString(wrapper);
            Console.WriteLine(result);
        }
    }
}