using System;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person _person;
        private PrintingConfig<Person> _printer;
        
        [SetUp]
        public void SetUp()
        {
            _person = new Person
            {
                Name = "Frank", 
                Surname = "Sinatra",
                Age = 19000000,
                Id = new Guid(),
                Height = 180.5,
                Money = 300,
            };
            
            _printer = ObjectPrinter.For<Person>();
        }
        
        [Test]
        public void Demo()
        {
                //1. Исключить из сериализации свойства определенного типа
                //2. Указать альтернативный способ сериализации для определенного типа
                //3. Для числовых типов указать культуру
                //4. Настроить сериализацию конкретного свойства
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                //6. Исключить из сериализации конкретного свойства

            _printer
                .Exclude<long>()
                .Exclude(p => p.Id);
            
            Console.WriteLine(_printer.PrintToString(_person));

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }

        [Test]
        public void Exclude_ShouldExcludeGivenProperty()
        {
            _printer
                .Exclude<long>()
                .Exclude(p => p.Id)
                .Select<double>().Serialize(n => Math.Round(n).ToString())
                // .Select(x => x.Name).Serialize(s => s.ToLower())
                .SetCulture<int>(CultureInfo.CurrentCulture)
                .SliceStrings(2);
            
            Console.WriteLine(_printer.PrintToString(_person));
        }
    }
}