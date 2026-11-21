using NUnit.Framework;
using System.Globalization;
using FluentAssertions;
using System;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private CultureInfo _setCultureInfo = CultureInfo.InvariantCulture;

        [Test]
        public void DefaultPrint()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var str = ObjectPrinter.For<Person>().PrintToString(person);

            str.Should().Contain("Alex");
            str.Should().Contain("19");
        }

        [Test]
        public void ExcludeType()
        {
            //1. Исключить из сериализации свойства определенного типа
            var person = new Person { Name = "Alex", Age = 19 };

            var str = ObjectPrinter.For<Person>()
                .ExcludeType(typeof(int))
                .PrintToString(person);

            str.Should().NotContain("Age");
            str.Should().Contain("Alex");
        }

        [Test]
        public void AlternativeSerialization()
        {
            //2. Указать альтернативный способ сериализации для определенного типа
            var person = new Person { Name = "Alex", Age = 19 };

            var str = ObjectPrinter.For<Person>()
                .AlternativeSerialization(typeof(int), i => i * 2)
                .PrintToString(person);

            str.Should().Contain("38");
            str.Should().NotContain("Age = 19");
        }

        [Test]
        public void SetNumericCulture()
        {
            //3. Для числовых типов указать культуру
            var person = new Person { Height = 1.5 };

            var str = ObjectPrinter.For<Person>()
                .SetNumericCulture(_setCultureInfo)
                .PrintToString(person);

            str.Should().Contain("1.5");
        }

        [Test]
        public void SetMemberSerialization_Using()
        {
            //4. Настроить сериализацию конкретного свойства
            var person = new Person { Name = "Alex" };

            var str = ObjectPrinter.For<Person>()
                .SetMemberSerialization(p => p.Name)
                .Using(n => n.ToUpper())
                .PrintToString(person);

            str.Should().Contain("ALEX");
        }

        [Test]
        public void SetMemberSerialization_TerminalPrint()
        {
            //4. Настроить сериализацию конкретного свойства
            var person = new Person { Name = "Alex", Age = 19 };

            var str = ObjectPrinter.For<Person>()
                .SetMemberSerialization(p => p.Name)
                .PrintToString(person);

            str.Should().Contain("Alex");
        }

        [Test]
        public void Truncate()
        {
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            var person = new Person { Name = "Alex" };

            var str = ObjectPrinter.For<Person>()
                .Truncate(3)
                .PrintToString(person);

            str.Should().Contain("Ale");
            str.Should().NotContain("Alex");
        }

        [Test]
        public void ExcludeMember()
        {
            //6. Исключить из сериализации конкретного свойства
            var person = new Person { Name = "Alex", Age = 19 };

            var str = ObjectPrinter.For<Person>()
                .ExcludeMember(p => p.Age)
                .PrintToString(person);

            str.Should().NotContain("Age");
            str.Should().Contain("Alex");
        }

        [Test]
        public void StandartSerialization_AndExtensionSugar()
        {
            //7. Стандартная сериализация
            var person = new Person { Name = "Alex", Age = 19 };

            var printerStandartSerialization = ObjectPrinter.For<Person>()
                .StandardSerialization();
            var str1 = printerStandartSerialization.PrintToString(person);
            
            str1.Should().Contain("Alex");

            //8. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var str2 = person.PrintToString();

            str2.Should().Contain("Alex");
        }

        [Test]
        public void FullChain_FluentStyle()
        {
            var person = new Person { Name = "Alexander", Age = 19, Height = 1.5 };

            var s = ObjectPrinter.For<Person>()
                .SetMemberSerialization(p => p.Name)
                .Using(n => n.ToUpper())
                .SetNumericCulture(CultureInfo.InvariantCulture)
                .Truncate(5)
                .ExcludeType(typeof(Guid))
                .PrintToString(person);

            s.Should().Contain("Name = ALEXA"); // UPPER + truncate 5
            s.Should().Contain("Age = 19");
            s.Should().NotContain("Id");
        }
    }
}