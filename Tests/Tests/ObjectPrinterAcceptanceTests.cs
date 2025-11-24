using System.Globalization;
using ObjectPrinting;
using ObjectPrinting.PrintingConfigs.Extensions;
using Tests.TestEntities;

namespace Tests.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19 };

            var printer = ObjectPrinter.For<Person>()
                .Excluding<Guid>() // 1. Исключить из сериализации свойства определенного типа                      
                .Printing<int>().Using(i => $"INT: {i}") // 2. Указать альтернативный способ сериализации для определенного типа    
                .Printing<double>().Using(CultureInfo.InvariantCulture) // 3. Для числовых типов указать культуру
                .Printing(p => p.Age).Using(a => $"AGE : {a}") // 4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Name).TrimToLength(10) // 5. Настроить обрезание строковых свойств
                .Excluding(p => p.Age) // 6. Исключить конкретное свойство
                .Create(); 

            var s1 = printer.PrintToString(person);
        
            var s2 = person.Print(); // 7. Синтаксический сахар — метод расширения
        
            var s3 = person.Print(p => p
                    .Printing<double>().Using(CultureInfo.GetCultureInfo("ru-RU")) // 8. ...с конфигурированием
            );
        }
    }
}