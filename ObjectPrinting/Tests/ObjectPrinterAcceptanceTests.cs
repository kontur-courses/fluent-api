using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 12.1, Nickname = "OG Alex" };

            var printer = ObjectPrinter.For<Person>()
                            .Excluding<Guid>()
                            .Configure<int>().Using(number => $"{number} 67")
                            .Configure<double>().WithCulture(new CultureInfo("ru-RU"))
                            .Configure(person => person.Name).Using(name => $"<{name}>")
                            .Configure(person => person.Name).Truncate(2)
                            .Excluding(person => person.Nickname);
            //1. Исключить из сериализации свойства определенного типа
            //2. Указать альтернативный способ сериализации для определенного типа
            //3. Для числовых типов указать культуру
            //4. Настроить сериализацию конкретного свойства
            //5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
            //6. Исключить из сериализации конкретного свойства

            string s1 = printer.PrintToString(person);
            Assert.Pass(s1);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию        
            //8. ...с конфигурированием
        }
        [Test]
        public void Prints_public_properties_and_fields_on_separate_lines()
        {
            var person = new Person
            {
                Name = "Alex",
                Age = 19,
                Height = 180,
                Nickname = "OG Alex"
            };

            var result = ObjectPrinter.For<Person>().PrintToString(person);

            var expected = string.Join(Environment.NewLine,
                "Person:",
                $"\tId = {Guid.Empty}",
                "\tName = Alex",
                "\tHeight = 180",
                "\tAge = 19",
                "\tNickname = OG Alex");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Skips_properties_without_a_public_getter()
        {
            var sample = new PropertyGetterSample
            {
                Readable = "visible",
                SetterWithPrivateGetter = "hidden"
            };

            var result = ObjectPrinter.For<PropertyGetterSample>().PrintToString(sample);

            var expected = string.Join(Environment.NewLine,
                "PropertyGetterSample:",
                "\tReadable = visible");

            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(true)]
        [TestCase((byte)12)]
        [TestCase((sbyte)-12)]
        [TestCase('R')]
        [TestCase(12.5d)]
        [TestCase(12.5f)]
        [TestCase((short)-12)]
        [TestCase((ushort)12)]
        [TestCase(12)]
        [TestCase(12u)]
        [TestCase(12L)]
        [TestCase(12UL)]
        [TestCase("scalar")]
        [TestCaseSource(nameof(ScalarValuesNotAllowedInAttributes))]
        public void Prints_scalar_values_as_values(object value)
        {
            var sample = new ScalarSample { Value = value };

            var result = ObjectPrinter.For<ScalarSample>().PrintToString(sample);
            var expected = $"ScalarSample:{Environment.NewLine}\tValue = {value}";

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Excludes_members_of_configured_type()
        {
            var person = new Person
            {
                Id = Guid.NewGuid(),
                Name = "Alex"
            };

            var result = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .PrintToString(person);

            var expected = string.Join(Environment.NewLine,
                "Person:",
                "\tName = Alex",
                "\tHeight = 0",
                "\tAge = 0",
                "\tNickname = ");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Uses_custom_serializer_for_configured_type()
        {
            var person = new Person { Age = 19 };

            var result = ObjectPrinter.For<Person>()
                .Configure<int>().Using(number => $"{number} y.o.")
                .PrintToString(person);

            var expected = string.Join(Environment.NewLine,
                "Person:",
                $"\tId = {Guid.Empty}",
                "\tName = null",
                "\tHeight = 0",
                "\tAge = 19 y.o.",
                "\tNickname = ");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Uses_custom_serializer_for_selected_property_only()
        {
            var person = new Person
            {
                Name = "Alex",
                Nickname = "OG Alex"
            };

            var result = ObjectPrinter.For<Person>()
                .Configure(person => person.Name).Using(name => $"<{name}>")
                .PrintToString(person);

            var expected = string.Join(Environment.NewLine,
                "Person:",
                $"\tId = {Guid.Empty}",
                "\tName = <Alex>",
                "\tHeight = 0",
                "\tAge = 0",
                "\tNickname = OG Alex");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Uses_custom_serializer_for_selected_field_only()
        {
            var person = new Person
            {
                Name = "Alex",
                Nickname = "OG Alex"
            };

            var result = ObjectPrinter.For<Person>()
                .Configure(person => person.Nickname).Using(nickname => $"[{nickname}]")
                .PrintToString(person);

            var expected = string.Join(Environment.NewLine,
                "Person:",
                $"\tId = {Guid.Empty}",
                "\tName = Alex",
                "\tHeight = 0",
                "\tAge = 0",
                "\tNickname = [OG Alex]");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Uses_configured_culture_for_formattable_values()
        {
            var person = new Person { Height = 12.5 };

            var result = ObjectPrinter.For<Person>()
                .Configure<double>().WithCulture(new CultureInfo("ru-RU"))
                .PrintToString(person);

            var expected = string.Join(Environment.NewLine,
                "Person:",
                $"\tId = {Guid.Empty}",
                "\tName = null",
                "\tHeight = 12,5",
                "\tAge = 0",
                "\tNickname = ");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Truncates_selected_string_property_only()
        {
            var person = new Person
            {
                Name = "Alexander",
                Nickname = "OG Alex"
            };

            var result = ObjectPrinter.For<Person>()
                .Configure(person => person.Name)
                .Truncate(4)
                .PrintToString(person);

            var expected = string.Join(Environment.NewLine,
                "Person:",
                $"\tId = {Guid.Empty}",
                "\tName = Alex",
                "\tHeight = 0",
                "\tAge = 0",
                "\tNickname = OG Alex");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Prints_cycle_between_objects_without_infinite_recursion()
        {
            var first = new Node { Name = "first" };
            var second = new Node { Name = "second" };
            first.Next = second;
            second.Next = first;

            var result = ObjectPrinter.For<Node>().PrintToString(first);

            var expected = string.Join(Environment.NewLine,
                "Node:",
                "\tName = first",
                "\tNext = Node:",
                "\t\tName = second",
                "\t\tNext = [Циклическая ссылка: Node]",
                "\t\tLeft = null",
                "\t\tRight = null",
                "\tLeft = null",
                "\tRight = null");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Prints_shared_reference_in_each_branch_without_marking_it_as_cycle()
        {
            var shared = new Node { Name = "shared" };
            var root = new Node { Left = shared, Right = shared };

            var result = ObjectPrinter.For<Node>().PrintToString(root);

            var expected = string.Join(Environment.NewLine,
                "Node:",
                "\tName = ",
                "\tNext = null",
                "\tLeft = Node:",
                "\t\tName = shared",
                "\t\tNext = null",
                "\t\tLeft = null",
                "\t\tRight = null",
                "\tRight = Node:",
                "\t\tName = shared",
                "\t\tNext = null",
                "\t\tLeft = null",
                "\t\tRight = null");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Prints_items_from_array_property()
        {
            var sample = new CollectionSample
            {
                ArrayItems = new[] { "array-first", "array-second" }
            };

            var result = ObjectPrinter.For<CollectionSample>().PrintToString(sample);

            var expected = string.Join(Environment.NewLine,
                "CollectionSample:",
                "\tArrayItems = [",
                "\t\tarray-first,",
                "\t\tarray-second",
                "\t]",
                "\tListItems = []",
                "\tDictionaryItems = {}");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Prints_items_from_list_property()
        {
            var sample = new CollectionSample
            {
                ListItems = new List<string> { "list-first", "list-second" }
            };

            var result = ObjectPrinter.For<CollectionSample>().PrintToString(sample);

            var expected = string.Join(Environment.NewLine,
                "CollectionSample:",
                "\tArrayItems = []",
                "\tListItems = [",
                "\t\tlist-first,",
                "\t\tlist-second",
                "\t]",
                "\tDictionaryItems = {}");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Prints_keys_and_values_from_dictionary_property()
        {
            var sample = new CollectionSample
            {
                DictionaryItems = new Dictionary<string, string>
                {
                    ["key"] = "value",
                    ["second-key"] = "second-value"
                }
            };

            var result = ObjectPrinter.For<CollectionSample>().PrintToString(sample);

            var expected = string.Join(Environment.NewLine,
                "CollectionSample:",
                "\tArrayItems = []",
                "\tListItems = []",
                "\tDictionaryItems = {",
                "\t\tkey: value,",
                "\t\tsecond-key: second-value",
                "\t}");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void Indents_objects_inside_collections_once_per_level()
        {
            var sample = new NodeCollectionSample
            {
                Nodes = new List<Node> { new() { Name = "nested" } }
            };

            var result = ObjectPrinter.For<NodeCollectionSample>().PrintToString(sample);

            var expected = string.Join(Environment.NewLine,
                "NodeCollectionSample:",
                "\tNodes = [",
                "\t\tNode:",
                "\t\t\tName = nested",
                "\t\t\tNext = null",
                "\t\t\tLeft = null",
                "\t\t\tRight = null",
                "\t]");

            Assert.That(result, Is.EqualTo(expected));
        }

        private sealed class Node
        {
            public string Name { get; set; } = string.Empty;
            public Node? Next { get; set; }
            public Node? Left { get; set; }
            public Node? Right { get; set; }
        }

        private sealed class CollectionSample
        {
            public string[] ArrayItems { get; set; } = Array.Empty<string>();
            public List<string> ListItems { get; set; } = new();
            public Dictionary<string, string> DictionaryItems { get; set; } = new();
        }

        private sealed class NodeCollectionSample
        {
            public List<Node> Nodes { get; set; } = new();
        }

        private sealed class PropertyGetterSample
        {
            public string Readable { get; set; } = string.Empty;
            public string SetterWithPrivateGetter { private get; set; } = string.Empty;
        }

        private sealed class ScalarSample
        {
            public object? Value { get; set; }
        }

        private static readonly TestCaseData[] ScalarValuesNotAllowedInAttributes =
        {
            new TestCaseData(12.5m),
            new TestCaseData((Half)12.5f),
            new TestCaseData(new IntPtr(12)),
            new TestCaseData(new UIntPtr(12u)),
            new TestCaseData((Int128)12),
            new TestCaseData((UInt128)12),
            new TestCaseData(new BigInteger(12)),
            new TestCaseData(new Complex(1, 2)),
            new TestCaseData(Guid.Parse("d85b1407-351d-4694-9392-03acc5870eb1")),
            new TestCaseData(new DateTime(2025, 1, 2, 3, 4, 5)),
            new TestCaseData(new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.Zero)),
            new TestCaseData(new DateOnly(2025, 1, 2)),
            new TestCaseData(new TimeOnly(3, 4, 5)),
            new TestCaseData(new TimeSpan(1, 2, 3))
        };
    }
}
