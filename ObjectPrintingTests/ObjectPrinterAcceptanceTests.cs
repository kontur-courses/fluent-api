using System.Globalization;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Extensions;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person
            {
                Name = "Alex", Age = 19, Surname = "Vasilyev", Weight = 70.3, Height = 170.9,
                Friends = new List<Person>{new() {Surname = "sd"}, null}
            };

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .ExcludeProperty<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .ChangeSerializationFor<Guid>()
                .To(x => "hello")
                //3. Для числовых типов указать культуру
                .ChangeSerializationFor<double>()
                .To(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .ChangeSerializationFor(x => x.Name)
                .To(x => "Another")
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .ChangeSerializationFor(x => x.Surname)
                .ToTrimmedLength(3)
                //6. Исключить из сериализации конкретного свойства
                .ExcludeProperty(x => x.Height);
            
            var serializedObject = printer.PrintToString(person);
            Console.WriteLine(serializedObject);
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            serializedObject = person.PrintToString();
            Console.WriteLine(serializedObject);
            //8. ...с конфигурированием
            serializedObject = person.PrintToString(s => s.ExcludeProperty(x => x.Name));
            Console.WriteLine(serializedObject);
        }
    }
}