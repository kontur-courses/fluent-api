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
            person = new Person {Name = "Alex", Age = 19, Height = 180 };
        }
        [Test]
        public void Demo()
        {
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<double>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(num => num.ToString())
                //3. Для числовых типов указать культуру
                .Printing<int>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Age).Using(age => age.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing<string>(p => p.Name).Trim(10)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Age);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }

        [Test]
        public void ExcludeInt()
        {
            var printer = ObjectPrinter.For<Person>().Exclude<int>();
            var result = printer.PrintToString(person);
            Console.WriteLine(result);
            Assert.False(result.Contains("Age = 19"));
        }

        [Test]
        public void ExcludeId()
        {
            var printer = ObjectPrinter.For<Person>().Exclude(p => p.Id);
            var result = printer.PrintToString(person);
            Console.WriteLine(result);
            Assert.False(result.Contains("Id"));
        }

        [Test]
        public void TypeAndProperty_Using()
        {
            var printer = ObjectPrinter.For<Person>()
                .Printing<int>().Using(num => "Ы " + num)
                .Printing(p => p.Name).Using(n => "ИМЯ: " + n);
            Console.WriteLine(printer.PrintToString(person));
        }
    }
}