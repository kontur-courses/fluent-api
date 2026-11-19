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
            var person = new Person { Name = "Alex", Age = 19, Height = 12.1 };

            var printer = ObjectPrinter.For<Person>()
                            .Excluding<Guid>()
                            .Using<int>(number => number.ToString() + " 67")
                            .WithCulture<double>(new CultureInfo("ru-RU"))
                            .Using(person => person.Name, name => $"<{name}>");
            //1. Исключить из сериализации свойства определенного типа
            //2. Указать альтернативный способ сериализации для определенного типа
            //3. Для числовых типов указать культуру
            //4. Настроить сериализацию конкретного свойства
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            //6. Исключить из сериализации конкретного свойства

            string s1 = printer.PrintToString(person);
            Assert.Pass(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }
        [Test]
        public void Prints_public_properties_and_fields()
        {
            var person = new Person
            {
                Name = "Alex",
                Age = 19,
                Height = 180
            };

            var result = ObjectPrinter.For<Person>().PrintToString(person);

            Assert.That(result, Does.Contain("Name = Alex" + Environment.NewLine));
            Assert.That(result, Does.Contain("Age = 19" + Environment.NewLine));
            Assert.That(result, Does.Contain("Height = 180" + Environment.NewLine));
        }

        [Test]
        public void Excludes_members_of_configured_type()
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                Name = "Alex"
            };

            var result = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .PrintToString(person);

            Assert.That(result, Does.Not.Contain("Id ="));
            Assert.That(result, Does.Contain("Name = Alex" + Environment.NewLine));
        }

        [Test]
        public void Uses_custom_serializer_for_configured_type()
        {
            var person = new Person { Age = 19 };

            var result = ObjectPrinter.For<Person>()
                .Using<int>(number => $"age: {number}")
                .PrintToString(person);

            Assert.That(result, Does.Contain("Age = age: 19" + Environment.NewLine));
        }

        [Test]
        public void Uses_custom_serializer_for_selected_property_only()
        {
            var person = new Person
            {
                Name = "Alex",
                Nickname = "OG Alex"
            };

            var result = ObjectPrinter.For<Person>()
                .Using(person => person.Name, name => $"<{name}>")
                .PrintToString(person);

            Assert.That(result, Does.Contain("Name = <Alex>" + Environment.NewLine));
            Assert.That(result, Does.Contain("Nickname = OG Alex" + Environment.NewLine));
        }

        [Test]
        public void Uses_custom_serializer_for_selected_field_only()
        {
            var person = new Person
            {
                Name = "Alex",
                Nickname = "OG Alex"
            };

            var result = ObjectPrinter.For<Person>()
                .Using(person => person.Nickname, nickname => $"[{nickname}]")
                .PrintToString(person);

            Assert.That(result, Does.Contain("Nickname = [OG Alex]" + Environment.NewLine));
            Assert.That(result, Does.Contain("Name = Alex" + Environment.NewLine));
        }

        [Test]
        public void Uses_configured_culture_for_formattable_values()
        {
            var person = new Person { Height = 12.5 };

            var result = ObjectPrinter.For<Person>()
                .WithCulture<double>(new CultureInfo("ru-RU"))
                .PrintToString(person);

            Assert.That(result, Does.Contain("Height = 12,5" + Environment.NewLine));
        }

    }
}
