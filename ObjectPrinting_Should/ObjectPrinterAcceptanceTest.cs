using System;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person() { Name = "Ilya", Age = 19, Height = 181.5 };
            var friend = new Person() { Name = "Kirill", Friend = person, Age = 21, Height = 182.8 };

            var testList = new List<string> { "первый", "второй", "третий", "четвертый"};
            var testArray = new[] { 1, 2, 3, 4, 5 };
            var testdictionary = new Dictionary<string, double> { { "Первый ключ", 1 }, { "Второй ключ", 2 } };

            person.Friend = friend;

            var printer = ObjectPrinter.For<Person>()
                //1. Исключение из сериализации свойства/поля определенного типа
                .Excluding<Guid>()
                //2. Альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для всех типов, имеющих культуру, есть возможность ее указать
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настройка сериализации конкретного свойства/ поля
                .Printing(x => x.Height).Using(x => (x + 10).ToString())
                //5. Возможность обрезания строк
                .Printing(p => p.Name).TrimmedToLength(2)
                //6. Исключение из сериализации конкретного свойства / поля
                .Excluding(p => p.Grades);

            var listPrinter = ObjectPrinter.For<List<string>>();
            var arrayPrinter = ObjectPrinter.For<int[]>();
            var dictionaryPrinter = ObjectPrinter.For<Dictionary<string, double>>();

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            string str = person.PrintToString();

            Console.WriteLine(printer.PrintToString(person));
            Console.WriteLine(listPrinter.PrintToString(testList));
            Console.WriteLine(arrayPrinter.PrintToString(testArray));
            Console.WriteLine(dictionaryPrinter.PrintToString(testdictionary));
            Console.WriteLine(str);
        }
    }
}