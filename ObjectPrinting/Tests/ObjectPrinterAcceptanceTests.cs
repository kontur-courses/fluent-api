using System.Globalization;
using System.IO;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Tests;
using FluentAssertions;
using ObjectExtensions = ObjectPrinting.Solved.ObjectExtensions;

namespace Tests
{
    [TestFixture]
    public class AcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person {Name = "Alex", Age = 19};

            ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Exclude<int>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .TypeSerializer<double>(d => $"My beautiful shiny double: {d}")
                //3. Для числовых типов указать культуру
                .Choose(o => o.Age).SetCulture(CultureInfo.CurrentCulture)
                //4. Настроить сериализацию конкретного свойства
                .Choose(o => o.Id).UseSerializer(i => i.ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Choose(o => o.Name).Trim(2)
                //6. Исключить из сериализации конкретного свойства
                .Choose(o => o.Height).Exclude()
                .PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            var string1 = person.Serialize();
            //8. ...с конфигурированием
            //var string2 =person.Serialize(d => d.Choose<int>().Exclude());
        }
        [Test]
        public void PrintToString_ShouldNotContainProperty_WhenPropertyExcluded()//TODO
        {
            var printer = ObjectPrinter.For<Person>().Exclude<int>();
            var firstPerson = new Person { Name = "Alexander", Age = 19, Height = 1.84};
            var firstPersonText = printer.PrintToString(firstPerson);
           firstPersonText.Should().NotContain("Age = ");
        }
        
        [Test]
        public void PrintToString_ShouldContainNewSerialization_AddedSerializationForType()//TODO
        {
            var printer = ObjectPrinter.For<Person>()
                .TypeSerializer<double>(d => $"My beautiful shiny double: {d}");
            var firstPerson = new Person { Name = "Alexander", Age = 19, Height = 1.84};
            
            var firstPersonText = printer.PrintToString(firstPerson);
            
            firstPersonText.Should().Contain("My beautiful shiny double:");
        }
        
        [Test]
        public void PrintToString_ShouldContainCorrectCulture_AddedCultureForProperty()//TODO
        {
            var printer = ObjectPrinter.For<Person>()
                .Choose(o => o.Age).SetCulture(CultureInfo.InvariantCulture);
            var firstPerson = new Person { Name = "Alexander", Age = 19, Height = 1.84};
            
            var firstPersonText = printer.PrintToString(firstPerson);
            
            firstPersonText.Should().Contain("My beautiful shiny double:");
        }
    }
}