using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Modules.Extensions;
using ObjectPrinting.Tests.Modules;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        private readonly Person person = new Person { Name = "Alex", Age = 19, Height = 1.8 };
        private readonly PropertyInfo[] personProperties = typeof(Person).GetProperties();
        private string result;

        [SetUp]
        public void SetUp()
        {
            result = "";
        }

        [TearDown]
        public void OnTearDown()
        {
            TestContext.WriteLine(result);
        }

        [Test]
        public void CompleteAcceptanceTest()
        {
            var printer = ObjectPrinter.For<Person>()
                //1. Исключить из сериализации свойства определенного типа
                .Excluding<Guid>()
                //2. Указать альтернативный способ сериализации для определенного типа
                .Printing<int>().Using(i => i.ToString("X"))
                //3. Для числовых типов указать культуру
                .Printing<double>().Using(CultureInfo.InvariantCulture)
                //4. Настроить сериализацию конкретного свойства
                .Printing(p => p.Height).Using(d => (d / 2).ToString())
                //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
                .Printing(p => p.Name).TrimmedToLength(3)
                //6. Исключить из сериализации конкретного свойства
                .Excluding(p => p.Age);

            var s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию
            var s2 = person.PrintToString();

            //8. ...с конфигурированием
            var s3 = person.PrintToString(s => s.Excluding(p => p.Age));

            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
        }

        private static readonly object[] TypeInstances =
        {
            "string",
            int.MaxValue,
            double.MaxValue,
            Guid.Empty
        };

        [TestCaseSource(nameof(TypeInstances))]
        public void ExcludeTypeFromSerialization<T>(T instance)
        {
            var instanceType = typeof(T);
            var printer = ObjectPrinter
                .For<Person>()
                .Excluding<T>();
            var includedProperties = personProperties
                .Where(p => p.PropertyType != instanceType)
                .Select(p => p.Name);
            var excludedProperties = personProperties
                .Where(p => p.PropertyType == instanceType)
                .Select(p => p.Name);

            result = printer.PrintToString(person);

            result.Should().NotContainAny(excludedProperties)
                .And.ContainAll(includedProperties);
        }

        [TestCaseSource(nameof(TypeInstances))]
        public void ChangeTypeSerialization<T>(T instance)
        {
            var instanceType = typeof(T);
            var printer = ObjectPrinter
                .For<Person>()
                .Printing<T>()
                .Using(s => s + "(Modified)");
            var expectedPropertiesValues = personProperties
                .Where(p => p.PropertyType == instanceType)
                .Select(p => p.GetValue(person).ToString() + "(Modified)");

            result = printer.PrintToString(person);

            result.Should().ContainAll(expectedPropertiesValues);
        }

        [Test]
        public void ChangeCultureForNumbers()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Printing<double>()
                .Using(CultureInfo.InvariantCulture);

            result = printer.PrintToString(person);

            result.Should().Be("Person\r\n" +
                               "\tId = Guid\r\n" +
                               "\tName = Alex\r\n" +
                               "\tHeight = 1.8\r\n" +
                               "\tAge = 19\r\n");
        }

        [Test]
        public void ChangePropertiesSerialization()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Printing(p => p.Name)
                .Using(s => s + "(Modified Name)")
                .Printing(p => p.Id)
                .Using(s => s + "(Modified Id)")
                .Printing(p => p.Age)
                .Using(s => s + "(Modified Age)")
                .Printing(p => p.Height)
                .Using(s => s + "(Modified Height)");

            result = printer.PrintToString(person);

            result.Should().Be("Person\r\n" +
                               $"\tId = {person.Id}(Modified Id)\r\n" +
                              $"\tName = {person.Name}(Modified Name)\r\n" +
                               $"\tHeight = {person.Height}(Modified Height)\r\n" +
                               $"\tAge = {person.Age}(Modified Age)\r\n");
        }

        [Test]
        public void PrioritySerializationByProperties()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Printing(p => p.Age)
                .Using(s => s + "(Modified from property)")
                .Printing<int>()
                .Using(s => s + "(Modified from type)");

            result = printer.PrintToString(person);

            result.Should().Be("Person\r\n" +
                               "\tId = Guid\r\n" +
                               "\tName = Alex\r\n" +
                               "\tHeight = 1,8\r\n" +
                               "\tAge = 19(Modified from property)\r\n");
        }

        [Test]
        public void TrimmingStringProperties()
        {
            var personWithLongName = new Person { Name = new string('A', 100) };
            var printer = ObjectPrinter
                .For<Person>()
                .Printing(p => p.Name)
                .TrimmedToLength(1);

            result = printer.PrintToString(personWithLongName);

            result.Should().Be("Person\r\n" +
                               "\tId = Guid\r\n" +
                               "\tName = A\r\n" +
                               "\tHeight = 0\r\n" +
                               "\tAge = 0\r\n");

        }

        [Test]
        public void TrimmingString()
        {
            var stringClass = new StringClass { A = "A", B = "BB", C = "CCC" };
            var printer = ObjectPrinter
                .For<StringClass>()
                .Printing<string>()
                .TrimmedToLength(1);

            result = printer.PrintToString(stringClass);

            result.Should().Be("StringClass\r\n" +
                               "\tA = A\r\n" +
                               "\tB = B\r\n" +
                               "\tC = C\r\n");

        }

        private static ICollection[] Collections =
        {
            new List<string> {"0", "1", "2"},
            new[] {0, 1, 2},
            new Dictionary<int, byte> {{0, 0}, {1, 1}, {2, 2}},
            new Stack<double>(new double[] {0, 1, 2}),
            new Queue<string>(new[] {"first", "second"})
        };

        [TestCaseSource(nameof(Collections))]
        public void WorksWithCollections(IEnumerable collection)
        {
            var type = collection.GetType();
            var printer = ObjectPrinter
                .For<IEnumerable>();
            var values = CollectItems(collection);

            result = printer.PrintToString(collection);

            result.Should().StartWith($"{type.Name}")
                .And.ContainAll(values);
        }

        private static IEnumerable<string> CollectItems(IEnumerable collection)
        {
            return from object item in collection select item.ToString();
        }

        [Test]
        public void NotThrowStackOverflowException_WhenClassHasCycledReferences()
        {
            var first = new CycleClass();
            var second = new CycleClass();
            first.Next = second;
            second.Next = first;
            var printer = ObjectPrinter
                .For<CycleClass>();

            Action printing = () => printer.PrintToString(first);

            printing.Should().NotThrow<StackOverflowException>();
        }

        [Test]
        public void PrintInformativeMessage_WhenReachingCycles()
        {
            var first = new CycleClass();
            var second = new CycleClass();
            first.Next = second;
            second.Next = first;
            var printer = ObjectPrinter
                .For<CycleClass>();

            result = printer.PrintToString(first);

            result.Should().Be("CycleClass\r\n" +
                               "\tNext = CycleClass\r\n" +
                               "\t\tNext = reached cycle\r\n");
        }

        [Test]
        public void PrintInformativeMessage_WhenNestingOverflow()
        {
            var nesting = NestingClass.CreateNesting(3);
            var printer = ObjectPrinter
                .For<NestingClass>()
                .WithNestingLevel(2);

            result = printer.PrintToString(nesting);

            result.Should().Be("NestingClass\r\n" +
                               "\tSon = NestingClass\r\n" +
                               "\t\tSon = reached max nesting level\r\n");
        }

        [Test]
        public void PrintNullProperties()
        {
            var noNamePerson = new Person();
            var printer = ObjectPrinter.For<Person>();

            result = printer.PrintToString(noNamePerson);

            result.Should().Be("Person\r\n" +
                               "\tId = Guid\r\n" +
                               "\tName = null\r\n" +
                               "\tHeight = 0\r\n" +
                               "\tAge = 0\r\n");
        }

        private static IEnumerable<object> FundamentalObjects()
        {
            yield return "string";
            yield return 0;
            yield return 1.1;
            yield return 2L;
            yield return 3.3f;
            yield return DateTime.Now;
            yield return TimeSpan.Zero;
        }

        [TestCaseSource(nameof(FundamentalObjects))]
        public void PrintFundamentalObjects<T>(T obj)
        {
            var printer = ObjectPrinter.For<T>();

            result = printer.PrintToString(obj);

            result.Should().Be(obj.ToString());
        }

        [Test]
        public void BeReusableForSameObj()
        {
            var printer = ObjectPrinter
                .For<Person>()
                .Printing(p => p.Name)
                .Using(s => s + "(X)");

            var first = printer.PrintToString(person);
            var second = printer.PrintToString(person);
            result = first + second;

            first.Should().BeEquivalentTo(second);
        }
    }
}