using System.Globalization;
using FluentAssertions;
using ObjectPrinting.Extensions;
using ObjectPrinting.Models;


namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 190.21 };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .For<Guid>().Exclude()
                //2. Указать альтернативный способ сериализации для определенного типа
                .For<double>().ChangeSerialization(d => d.ToString("e2"))
                //3. Для числовых типов указать культуру
                .For<DateTime>().UseCulture(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .For(p => p.Age).ChangeSerialization(d => d.ToString("F"))
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .For(p => p.Name).SetLength(3)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Age);

            var s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();

            //8. ...с конфигурированием
            var s3 = person.PrintToString(s => s.Exclude(p => p.Age));
            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }
    }
}