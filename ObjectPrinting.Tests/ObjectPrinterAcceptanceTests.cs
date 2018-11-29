using System.Globalization;
using NUnit.Framework;

// ReSharper disable CommentTypo

namespace ObjectPrinting.Tests
{
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person {Name = "Alex", Age = 19};

            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Serializing<string>().Using(x => x)
                //3. Для числовых типов указать культуру
                .Serializing<int>().Using(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Serializing(p => p.Height).Using(s => s.ToString(CultureInfo.CurrentCulture))
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serializing(p => p.Name).TrimToLength(6)
                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Id);

            printer.PrintToString(person);
        }

        [Test]
        public void SyntaxSugarDemo()
        {
            var person = new Person {Name = "Alex", Age = 19};

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            person.Serialize();
        }

        [Test]
        public void ConfigurableSyntaxSugarDemo()
        {
            var person = new Person {Name = "Alex", Age = 19};

            //8. ...с конфигурированием
            person.Serialize(x => x.Serializing(p => p.Name).TrimToLength(2));
        }
    }
}
