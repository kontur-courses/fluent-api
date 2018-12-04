using System;
using System.Collections.Generic;
using System.Globalization;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter), typeof(FileLauncherReporter))]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void PrintToString_AddFinalType()
        {
            var product = new
            {
                Name = "paper box",
                Price = 12,
                Weight = 40
            };

            Approvals.Verify(product.PrintToString(cfg => cfg.AddFinalType(product.GetType())));
        }

        [Test]
        public void PrintToString_Array()
        {
            var array = new int[10];
            Approvals.Verify(array.PrintToString());
        }

        [Test]
        public void PrintToString_CircularReference_NotHaveStackOverFlow()
        {
            var father = new Person
            {
                Height = 60,
                Age = 50,
                Name = "John",
                Id = Guid.Parse("2CBFC598-C3BD-467E-987F-71EE87363566")
            };
            var son = new Person
            {
                Height = 60,
                Age = 20,
                Name = "Alex",
                Id = Guid.Parse("5BED1F5E-17CD-4862-A15D-8C80C71733BE"),
                Father = father
            };
            father.Father = son;
            Approvals.Verify(son.PrintToString());
        }

        [Test]
        public void PrintToString_Class()
        {
            var student = new Person
            {
                Height = 60,
                Age = 20,
                Name = "Alex",
                Id = Guid.Parse("E6AA2CF0-D273-4016-BADA-DE415AE51C35")
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

            Approvals.Verify(printer.PrintToString(student));
        }

        [Test]
        public void PrintToString_Culture()
        {
            var longNumber = 1000000000L;
            var stringToPrint =
                longNumber.PrintToString(x => x.Serializing<long>().Using(CultureInfo.GetCultureInfo("de-De")));
            Approvals.Verify(stringToPrint);
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
            Approvals.Verify(dictionary.PrintToString());
        }

        [Test]
        public void PrintToString_DictionaryWithNestedList()
        {
            var dictionary = new Dictionary<int, List<string>>
            {
                {25, new List<string> {"hello", "its", "me"}},
                {48, new List<string> {"i", "was", "wondering.."}}
            };

            Approvals.Verify(dictionary.PrintToString());
        }

        [Test]
        public void PrintToString_EmptyDictionary()
        {
            var dictionary = new Dictionary<int, string>();
            Approvals.Verify(dictionary.PrintToString());
        }

        [Test]
        public void PrintToString_EmptyList()
        {
            var list = new List<string>();
            Approvals.Verify(list.PrintToString());
        }

        [Test]
        public void PrintToString_FastSerializeWithConfig()
        {
            var list = new HashSet<int>
            {
                1, 5, 89, 52, 8, 5
            };
            var stringToPrint = list.PrintToString(cfg => cfg.Serializing<int>().Using(s => "number = " + s));
            Approvals.Verify(stringToPrint);
        }

        [Test]
        public void PrintToString_HashSet()
        {
            var list = new HashSet<int>
            {
                1, 5, 89, 52, 8, 5
            };

            Approvals.Verify(list.PrintToString());
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

            Approvals.Verify(list.PrintToString());
        }

        [Test]
        public void PrintToString_ListWithNestedList()
        {
            var list = new List<List<string>>
            {
                new List<string> {"hello", "its", "me"},
                new List<string> {"i", "was", "wondering.."}
            };

            Approvals.Verify(list.PrintToString());
        }

        [Test]
        public void PrintToString_MaxDepth()
        {
            var father = new Person
            {
                Height = 60,
                Age = 50,
                Name = "John",
                Id = Guid.Parse("8CAE0910-8BAF-454C-9C0B-1AFAA609E8F3")
            };
            var son = new Person
            {
                Height = 60,
                Age = 20,
                Name = "Alex",
                Id = Guid.Parse("34342592-0E22-4841-A774-AA8CAB8B3631"),
                Father = father
            };
            father.Father = son;
            Approvals.Verify(son.PrintToString(config => config.SetMaxDepth(2)));
        }

        [Test]
        public void PrintToString_RemoveFinalType()
        {
            var str = "hello";
            Approvals.Verify(str.PrintToString(config => config.RemoveFinalType(typeof(string))));
        }

        [Test]
        public void PrintToString_TrimToLengthForType()
        {
            var stringToPrint = "hello";
            Approvals.Verify(
                stringToPrint.PrintToString(config => config.Serializing<string>().TrimmedToLength(2)));
        }

        [Test]
        public void PrintToString_WrongConfiguration_PrintErrors()
        {
            var product = new
            {
                Name = "paper box",
                Price = 12,
                Weight = 40
            };
            Approvals.Verify(product.PrintToString(config => config.Serializing(p => 10).Using(x => x + "!!!")));
        }
    }
}