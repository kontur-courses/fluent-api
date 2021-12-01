using System.Drawing;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private Person person;
        private PrintingConfig<Person> printer;

        [SetUp]
        public void InitializeVariables()
        {
            person = new Person {Name = "Alex", Age = 19, Height = 0.2};
            printer = ObjectPrinter.For<Person>();
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
            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
            foreach (var property in person.GetType().GetProperties())
            {
                printer.PrintToString(person).Should().Contain(property.Name);
            }
        }

        [Test]
        public void ExceptType_NotPrintSelectedType()
        {
            printer.ExceptType<int>().PrintToString(person).Should().NotContain("Age");
        }

        [Test]
        public void ExceptProperty_NotPrintSelectedProperty()
        {
            var point = new Point(1, 2);
            var pointPrinter = ObjectPrinter.For<Point>().ExceptProperty(point.GetType().GetProperties()[1]);

            pointPrinter.PrintToString(point).Should().NotContain("X");
        }

        [Test]
        public void Serialize_SetsAlternativeSerializationForSelectedType()
        {
            printer.Serialize<int>().Using(i => "xxx").PrintToString(person).Should().Contain("xxx");
        }

        [Test]
        public void Serialize_SetsCultureForNumbers()
        {
            printer.Serialize<double>()
                .Using(CultureInfo.CurrentCulture)
                .PrintToString(person)
                .Should()
                .Contain("Height = 0,2");
            printer.Serialize<double>()
                .Using(CultureInfo.InvariantCulture)
                .PrintToString(person)
                .Should()
                .Contain("Height = 0.2");
        }

        [Test]
        public void Serialize_SetsSerializationForSelectedProperty()
        {
            printer.Serialize(person.GetType().GetProperties()[0])
                .Using(x => "Id = lol")
                .PrintToString(person)
                .Should()
                .Contain("lol");
        }

        [Test]
        public void SettingCulture_HasLowerPriority_ThanAlternativeTypeSerialization()
        {
            printer.Serialize<double>()
                .Using(CultureInfo.InvariantCulture)
                .Serialize<double>()
                .Using(x => "a")
                .PrintToString(person)
                .Should()
                .Contain("Height = a");
        }

        [Test]
        public void AlternativeTypeSerialization_HasLowerPriority_ThanAlternativePropertySerialization()
        {
            printer.Serialize<double>()
                .Using(x => "a")
                .Serialize(person.GetType().GetProperties()[2])
                .Using(x => "b")
                .PrintToString(person)
                .Should()
                .Contain("Height = b");
        }

        [Test]
        public void AlternativePropertySerialization_HasLowerPriority_ThanExcluding()
        {
            printer.Serialize(person.GetType().GetProperties()[2])
                .Using(x => "b")
                .ExceptType<double>()
                .PrintToString(person)
                .Should()
                .NotContain("Height = b");
        }

        [Test]
        public void ResultNotDependsOnOrderOfCommands()
        {
            var firstOrderResult = printer.Serialize(person.GetType().GetProperties()[2])
                .Using(x => "b")
                .ExceptType<int>()
                .Serialize<string>()
                .Using(s => "a")
                .PrintToString(person);
            var secondOrderResult = printer.ExceptType<int>()
                .Serialize<string>()
                .Using(s => "a")
                .Serialize(person.GetType().GetProperties()[2])
                .Using(x => "b")
                .PrintToString(person);
            
            firstOrderResult.Should().BeEquivalentTo(secondOrderResult);
        }
    }
}