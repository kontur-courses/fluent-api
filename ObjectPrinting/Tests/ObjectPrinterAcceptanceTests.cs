using System;
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
            var person = new Person { Name = "Alex_ShouldntSeeIt", Age = 19, Surname = "Bronson" ,Height = 12.4};
            person.Parent = person;
            

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .ForType<Guid>().Exclude()
                //2. Указать альтернативный способ сериализации для определенного типа
                .ForType<int>().Using(i => i.ToString())
                //3. Для числовых типов указать культуру
                .ForType<double>().WithCulture(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .ForProperty(p => p.Name).TrimTo(4)
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .ForProperty(p => p.Surname).Using(s =>s.ToUpper() )
                //6. Исключить из сериализации конкретного свойства
                .ForProperty(p => p.Age).Exclude();


            string s1 = printer.PrintToString(person);
            Console.WriteLine(s1);
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию       
            var s2 = person.PrintToString();
            //8. ...с конфигурированием
            var s3 = person.PrintToString(config =>config.ForType<int>().Exclude() );
        }
    }
}