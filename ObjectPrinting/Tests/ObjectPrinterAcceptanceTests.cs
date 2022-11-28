using System;
using System.Collections.Generic;
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
        public Vehicle VehicleCar;
        private Person simplePerson;
        private Person childPerson;




        [SetUp]
        public void SetUp()
        {
            VehicleCar = new Vehicle("Audi", 230, 1400, 2005);

            simplePerson = new Person() { Age = 35, Height = 155, Id = new Guid(), Name = "Anna", Car = VehicleCar };
            childPerson = new Person()
            {
                Age = 8, Height = 86.34, Id = new Guid(), Name = "Seraphina", Parent = simplePerson
            };
            printedString = null;

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
            var person = new Person
            {
                Name = "Vladimir", Weight = 85, Age = 19, Height = 187.50, Id = Guid.Empty,
                Parent = new Person() { Age = 44, Name = "Anna", Height = 155.3 }
            };
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()// исключение типа Guid
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => $"full {i}") // все поля int кроме Age будет в формате "full {property}" 
                ////3. Для числовых типов указать культуру
                .Printing<double>().UsingWithFormatting(new CultureInfo("en")) // "." вместо ru=","
                ////4. Настроить сериализацию конкретного свойства
                .Printing(x => x.Age).Using(x => $"{x} Years") // все поля Age в формате "{Age} Years"
                ////5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(4) // все поля Name будут длинной не больше 4 символов
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

        [Test]
        public void PrintExcludingString_WhenHaveManyClasses()
        {
            var printer = ObjectPrinter.For<Person>();
            printedString = printer.PrintToString(childPerson);
            printedString.Should().Contain("AgeOfTheCar = 17");
            printer = ObjectPrinter.For<Person>().Excluding<Vehicle>();
            childPerson.TypeList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 9 };
            childPerson.TypeArray = new [] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 9 };
            childPerson.TypeDict = new Dictionary<int, object>() { [1] = 123 , [2]="rere"};
            childPerson.TypeSet = new HashSet<string>() { "effe", "nice hashset" };
            printedString = printer.PrintToString(childPerson);
            printedString.Should().NotContain("AgeOfTheCar = 17");
        }

        [Test]
        public void PrintCyclicReference()
        {
            simplePerson.Parent = childPerson;
            var printer = ObjectPrinter.For<Person>();
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().Contain("Parent = Cyclic reference");
        }

        [Test]
        public void IEnumerable()
        {
            var list = new List<object>() { 2, 23, 434, 3, 5, 4, 54, 5, 6, 6, 5, 65, 6 };
            var dict = new Dictionary<string, object>() { ["fef"] = 12, ["nicekey"]="yes", ["fe123f"] = 12, ["ffeef"] = childPerson, ["list"]=list };
            
            var printer= ObjectPrinter.For<IDictionary<string,object>>();
            printedString = printer.PrintToString(dict);

        }
    }
}