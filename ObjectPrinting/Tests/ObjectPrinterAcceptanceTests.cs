using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person {Name = "Alex", Age = 19};

            //1. Исключить из сериализации свойства определенного типа
            var printerExcludingType = ObjectPrinter.For<Person>().Excluding<string>();
            //2. Указать альтернативный способ сериализации для определенного типа
            var printerUsingSerializingForType = ObjectPrinter.For<Person>()
                .Serializing<DateTime>()
                .Using(d => d.ToString());
            //3. Для числовых типов указать культуру
            var printerUsingCultureForNumber = ObjectPrinter.For<Person>()
                    .UsingNumbersCulture(CultureInfo.CurrentCulture);
            //4. Настроить сериализацию конкретного свойства
            var printerSerializingForProperty = ObjectPrinter.For<Person>()
                .Serializing(p => p.Name).Using(d => d.ToString());
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            var printerTrimmingStringProperty = ObjectPrinter.For<Person>()
                .Serializing(p => p.Name).TrimmingToLength(6);
            //6. Исключить из сериализации конкретного свойства
            var printerExcludingProperty = ObjectPrinter.For<Person>().Excluding(p => p.Name);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            var defaultSerializingForObject = new Person().PrintToString();
            //8. ...с конфигурированием
            var SerializeWithConfiguration = new Person().Serialize().PrintToString();
        }
    }
}