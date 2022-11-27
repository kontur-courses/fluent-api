using System;
using System.Globalization;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private string printedString;
        [SetUp]
        public void SetUp()
        {
            printedString = "";
        }

        [TearDown]
        public void TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Failed)
                Console.WriteLine(printedString);
        }

        [Test]
        public void TestForCheckWorkSimpleScript()
        {
            var person = new Person { Name = "Vladimir",Weight = 85 ,Age = 19,Height = 187.50, Id = Guid.Empty, Parent = new Person(){Age = 44,Name = "Anna",Height = 155.3}};
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid?>()// исключение типа Guid  
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => $"full {i}")// все поля int кроме Age будет в формате "full {property}" 
                ////3. Для числовых типов указать культуру
                .Printing<double>().Using(new CultureInfo("en"))// "." вместо ","
                ////4. Настроить сериализацию конкретного свойства
                .Printing(x => x.Age).Using(x => $"{x} Years")// все поля Age в формате "{Age} Years"
                ////5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(4)// все поля Name будут длинной не больше 4 символов
                ////6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.HaveCar); // исключение поля HaveCar
            printedString = printer.PrintToString(person);
            printedString.Should().NotContain(person.Id.ToString());
            printedString.Should().Contain("Weight = full 85");
            printedString.Should().Contain("Height = 187.5");
            printedString.Should().NotContain(person.Name);
            printedString.Should().Contain("Name = Vlad");
            printedString.Should().NotContain(person.HaveCar.ToString());
        }
    }
}