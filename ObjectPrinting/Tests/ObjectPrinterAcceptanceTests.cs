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
        public void Prints_public_properties_and_fields()
        {
            var person = new Person
            {
                Name = "Alex",
                Age = 19,
                Height = 180
            };

            var result = ObjectPrinter.For<Person>().PrintToString(person);

            Assert.That(result, Does.Contain("Name = Alex" + Environment.NewLine));
            Assert.That(result, Does.Contain("Age = 19" + Environment.NewLine));
            Assert.That(result, Does.Contain("Height = 180" + Environment.NewLine));
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

            Assert.That(result, Does.Not.Contain("Id ="));
            Assert.That(result, Does.Contain("Name = Alex" + Environment.NewLine));
        }

        [Test]
        public void Uses_custom_serializer_for_configured_type()
        {
            var person = new Person { Age = 19 };

            var result = ObjectPrinter.For<Person>()
                .Configure<int>().Using(number => $"{number} y.o.")
                .PrintToString(person);

            Assert.That(result, Does.Contain("Age = 19 y.o." + Environment.NewLine));
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

            Assert.That(result, Does.Contain("Name = <Alex>" + Environment.NewLine));
            Assert.That(result, Does.EndWith("Nickname = OG Alex"));
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

            Assert.That(result, Does.EndWith("Nickname = [OG Alex]"));
            Assert.That(result, Does.Contain("Name = Alex" + Environment.NewLine));
        }

        [Test]
        public void Uses_configured_culture_for_formattable_values()
        {
            var person = new Person { Height = 12.5 };

            var result = ObjectPrinter.For<Person>()
                .Configure<double>().WithCulture(new CultureInfo("ru-RU"))
                .PrintToString(person);

            Assert.That(result, Does.Contain("Height = 12,5" + Environment.NewLine));
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

            Assert.That(result, Does.Contain("Name = Alex" + Environment.NewLine));
            Assert.That(result, Does.EndWith("Nickname = OG Alex"));
        }

        [Test]
        public void Prints_cycle_between_objects_without_infinite_recursion()
        {
            var first = new Node { Name = "first" };
            var second = new Node { Name = "second" };
            first.Next = second;
            second.Next = first;

            var result = ObjectPrinter.For<Node>().PrintToString(first);

            Assert.That(result, Does.Contain("Name = first" + Environment.NewLine));
            Assert.That(result, Does.Contain("Name = second" + Environment.NewLine));
            Assert.That(result, Does.Contain("[Циклическая ссылка: Node]"));
        }

        [Test]
        public void Prints_shared_reference_in_each_branch_without_marking_it_as_cycle()
        {
            var shared = new Node { Name = "shared" };
            var root = new Node { Left = shared, Right = shared };

            var result = ObjectPrinter.For<Node>().PrintToString(root);

            Assert.That(result, Does.Contain("Left = Node:"));
            Assert.That(result, Does.Contain("Right = Node:"));
            Assert.That(
                result.Split("Name = shared", StringSplitOptions.None).Length - 1,
                Is.EqualTo(2));
            Assert.That(result, Does.Not.Contain("[Циклическая ссылка:"));
        }

        [Test]
        public void Prints_items_from_array_property()
        {
            var sample = new CollectionSample
            {
                ArrayItems = new[] { "array-first", "array-second" }
            };

            var result = ObjectPrinter.For<CollectionSample>().PrintToString(sample);

            Assert.That(result, Does.Contain(
                $"ArrayItems = [{Environment.NewLine}\t\tarray-first" +
                $"{Environment.NewLine}\t\tarray-second{Environment.NewLine}\t]" +
                $"{Environment.NewLine}\tListItems ="));
        }

        [Test]
        public void Prints_items_from_list_property()
        {
            var sample = new CollectionSample
            {
                ListItems = new List<string> { "list-first", "list-second" }
            };

            var result = ObjectPrinter.For<CollectionSample>().PrintToString(sample);

            Assert.That(result, Does.Contain(
                $"ListItems = [{Environment.NewLine}\t\tlist-first" +
                $"{Environment.NewLine}\t\tlist-second{Environment.NewLine}\t]"));
        }

        [Test]
        public void Prints_keys_and_values_from_dictionary_property()
        {
            var sample = new CollectionSample
            {
                DictionaryItems = new Dictionary<string, string>
                {
                    ["key"] = "value"
                }
            };

            var result = ObjectPrinter.For<CollectionSample>().PrintToString(sample);

            Assert.That(result, Does.Contain(
                $"DictionaryItems = {{{Environment.NewLine}\t\tkey: value" +
                $"{Environment.NewLine}\t}}"));
            Assert.That(result, Does.Not.EndWith(Environment.NewLine));
        }

        [Test]
        public void Indents_objects_inside_collections_once_per_level()
        {
            var sample = new NodeCollectionSample
            {
                Nodes = new List<Node> { new() { Name = "nested" } }
            };

            var result = ObjectPrinter.For<NodeCollectionSample>().PrintToString(sample);

            Assert.That(result, Does.Contain(
                $"Nodes = [{Environment.NewLine}\t\tNode:" +
                $"{Environment.NewLine}\t\t\tName = nested"));
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
    }
}
