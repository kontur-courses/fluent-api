using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;
        
        [SetUp]
        public void SetUp()
        {
            person = new Person { Name = "Alexander", Age = 19, Height = 1.84 };
        }

        
        [Test]
        public void Demo()
        {
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<double>().Using(number => $"double {number.ToString()}")
                //3. Для числовых типов указать культуру
                .Printing<int>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Age).Using(number => $"int number {number.ToString()}")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).Trimmed(4)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Age);
            
            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию  
            string s2 = person.PrintToString();
            //8. ...с конфигурированием
            string s3 = person.PrintToString(p => p.Exclude<Guid>());
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        [Test]
        public void ObjectPrinter_ShouldBeImutable()
        {
            var firstPrinter = ObjectPrinter.For<Person>();
            var secondPrinter = firstPrinter.Exclude<double>();
            var thirdPrinter = firstPrinter.Exclude<int>();
            
            Console.WriteLine(firstPrinter.PrintToString(person));
            Console.WriteLine(secondPrinter.PrintToString(person));
            Console.WriteLine(thirdPrinter.PrintToString(person));
        }
    }
}