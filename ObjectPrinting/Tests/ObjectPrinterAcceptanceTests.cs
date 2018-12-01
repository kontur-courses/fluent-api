using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void PrintToString_CircularReference_NotHaveStackOverFlow()
        {
            var father = new Person
            {
                Height = 60,
                Age = 50,
                Name = "John",
                Id = Guid.NewGuid()
            };
            var son = new Person
            {
                Height = 60,
                Age = 20,
                Name = "Alex",
                Id = new Guid(),
                Father = father
            };
            father.Father = son;
            Console.WriteLine(son.PrintToString());
        }

        [Test]
        public void PrintToString_Class()
        {
            var student = new Person
            {
                Height = 60,
                Age = 20,
                Name = "Alex",
                Id = Guid.NewGuid()
            };
            var printer = ObjectPrinter.For<Person>()
//                1. Исключить из сериализации свойства определенного типа
                .Exclude<double>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Serializing<int>().Using(s => "##" + s.ToString())
//                //3. Для числовых типов указать культуру
                .Serializing<double>().Using(CultureInfo.CurrentCulture)
//                //4. Настроить сериализацию конкретного свойства
                .Serializing(p => p.Id).Using(s => s.ToString() + "sdf")
//                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Serializing(p => p.Name).TrimmedToLength(100)

//                //6. Исключить из сериализации конкретного свойства
                .Exclude(p => p.Id);

            Console.WriteLine(printer.PrintToString(student));
        }

        [Test]
        public void PrintToString_Culture()
        {
            var longNumber = 1000000000L;
            var stringToPrint =
                longNumber.PrintToString(x => x.Serializing<long>().Using(CultureInfo.GetCultureInfo("de-De")));
            Console.WriteLine(stringToPrint);
        }
        
        [Test]
        public void PrintToString_Dictionary()
        {
            var dictionary = new Dictionary<int, string>
            {
                {32, "hello"},
                {45, "its"},
                {5, "me"},
                {2, "i was"}
            };
            Console.WriteLine(dictionary.PrintToString());
        }

        [Test]
        public void PrintToString_DictionaryWithNestedList()
        {
            var dictionary = new Dictionary<int, List<string>>
            {
                {25, new List<string> {"hello", "its", "me"}},
                {48, new List<string> {"i", "was", "wondering.."}}
            };

            Console.WriteLine(dictionary.PrintToString());
        }

        [Test]
        public void PrintToString_Array()
        {
            var array = new int[10];
            Console.WriteLine(array.PrintToString());
        }

        [Test]
        public void PrintToString_EmptyDictionary()
        {
            var dictionary = new Dictionary<int, string>();
            Console.WriteLine(dictionary.PrintToString());
        }

        [Test]
        public void PrintToString_EmptyList()
        {
            var list = new List<string>();
            Console.WriteLine(list.PrintToString());
        }

        [Test]
        public void PrintToString_FastSerializeWithConfig()
        {
            var list = new HashSet<int>
            {
                1, 5, 89, 52, 8, 5
            };
            var stringToPrint = list.PrintToString(cfg => cfg.Serializing<int>().Using(s => "number = " + s));
            Console.WriteLine(stringToPrint);
        }

        [Test]
        public void PrintToString_HashSet()
        {
            var list = new HashSet<int>
            {
                1, 5, 89, 52, 8, 5
            };

            Console.WriteLine(list.PrintToString());
        }

        [Test]
        public void PrintToString_List()
        {
            var list = new List<string>
            {
                "hello",
                "its",
                "me",
                "i",
                "was",
                "wondering.."
            };

            Console.WriteLine(list.PrintToString());
        }

        [Test]
        public void PrintToString_ListWithNestedList()
        {
            var list = new List<List<string>>
            {
                new List<string> {"hello", "its", "me"},
                new List<string> {"i", "was", "wondering.."}
            };

            Console.WriteLine(list.PrintToString());
        }
    }
}