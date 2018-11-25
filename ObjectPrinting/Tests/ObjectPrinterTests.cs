using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        [Test]
        public void ObjectPrinter_OnExcludePropertyType_ExcludesPropertyWithGivenType()
        {
            var objectPrinter = ObjectPrinter.For<Person>();
            var person = new Person {Name = "Bob", Age = 100};
            var expected = string.Join(Environment.NewLine, new[]
            {
                "Person",
                "\tId = Guid",
                $"\tHeight = 0{Environment.NewLine}",
            });

            objectPrinter
                .Exclude<int>()
                .Exclude<string>()
                .PrintToString(person)
                .Should()
                .BeEquivalentTo(expected);
        }

        [Test]
        public void ObjectPrinter_OnSerializePropertyWithAlternativeWay_SerializesPropertyWithAlternativeWay()
        {
            var objectPrinter = ObjectPrinter.For<Person>();
            var person = new Person {Name = "Bob", Age = 100};
            var expected = string.Join(Environment.NewLine, new[]
            {
                "Person",
                "\tId = Guid",
                "\tName = 42",
                "\tHeight = 0",
                $"\tAge = 100{Environment.NewLine}",
            });

            objectPrinter
                .Printing<string>()
                .Using(s => "42")
                .PrintToString(person)
                .Should()
                .BeEquivalentTo(expected);
        }

        [Test]
        public void ObjectPrinter_OnSetCultureForNumericTypes_SetsCultureForNumericTypes()
        {
            var objectPrinter = ObjectPrinter.For<Person>();
            var person = new Person {Name = "Bob", Age = 100, Height = 186.6};
            var expected = string.Join(Environment.NewLine, new[]
            {
                "Person",
                "\tId = Guid",
                "\tName = Bob",
                "\tHeight = 186.6",
                $"\tAge = 100{Environment.NewLine}",
            });

            objectPrinter
                .Printing<double>()
                .Using(CultureInfo.InvariantCulture)
                .PrintToString(person)
                .Should()
                .BeEquivalentTo(expected);
        }

        [Test]
        public void
            ObjectPrinter_OnSerializeSpecificPropertyWithAlternativeWay_SerializesSpecificPropertyWithAlternativeWay()
        {
            var objectPrinter = ObjectPrinter.For<Person>();
            var person = new Person {Name = "Bob", Age = 100};
            var expected = string.Join(Environment.NewLine, new[]
            {
                "Person",
                "\tId = Guid",
                "\tName = bbb",
                "\tHeight = 0",
                $"\tAge = 100{Environment.NewLine}",
            });

            objectPrinter
                .Printing(p => p.Name)
                .Using(z => "bbb")
                .Printing<string>()
                .Using(z => "asd")
                .PrintToString(person)
                .Should()
                .BeEquivalentTo(expected);
        }

        [Test]
        public void ObjectPrinter_OnTrim_TrimsString()
        {
            var objectPrinter = ObjectPrinter.For<Person>();
            var person = new Person {Name = "Bob", Age = 100};
            var expected = string.Join(Environment.NewLine, new[]
            {
                "Person",
                "\tId = Guid",
                "\tName = Bo",
                "\tHeight = 0",
                $"\tAge = 100{Environment.NewLine}",
            });

            objectPrinter
                .Printing(p => p.Name)
                .Trim(2)
                .PrintToString(person)
                .Should()
                .BeEquivalentTo(expected);
        }

        [Test]
        public void ObjectPrinter_OnExcludeSpecificProperty_ExcludesSpecificProperty()
        {
            var objectPrinter = ObjectPrinter.For<Person>();
            var person = new Person {Name = "Bob", Age = 100};
            var expected = string.Join(Environment.NewLine, new[]
            {
                "Person",
                "\tId = Guid",
                "\tHeight = 0",
                $"\tAge = 100{Environment.NewLine}",
            });

            objectPrinter
                .Exclude(p => p.Name)
                .PrintToString(person)
                .Should()
                .BeEquivalentTo(expected);
        }

        [Test]
        public void ObjectPrinter_OnRecursiveLinks_SerializesRecursiveLinkAsCircular()
        {
            var objectPrinter = ObjectPrinter.For<Node>();
            var parent = new Node() {Value = 1};
            var node = new Node {Value = 1, Parent = parent};
            parent.Parent = node;
            var expected = string.Join(Environment.NewLine, new[]
            {
                "Node",
                "\tValue = 1",
                "\tParent = Node",
                "\t\tValue = 1",
                $"\t\tParent = {Constants.Circular}",
            });

            objectPrinter.PrintToString(node)
                .Should()
                .BeEquivalentTo(expected);
        }

        [Test]
        public void ObjectPrinter_OnCollection_WorksCorrectly()
        {
            var objectPrinter = ObjectPrinter.For<Player<int>>();
            var player = new Player<int> {Name = "Sasha", Scores = new List<int> {3, 2, 1}};

            var expected = string.Join(Environment.NewLine, new[]
            {
                "Player`1",
                "\tName = Sasha",
                "\tScores = List`1",
                "\t\tElement 0 = 3",
                "\t\tElement 1 = 2",
                $"\t\tElement 2 = 1{Environment.NewLine}",
            });

            objectPrinter
                .PrintToString(player)
                .Should()
                .BeEquivalentTo(expected);
        }

        [Test]
        public void ObjectPrinter_OnDictionaryOfObjects_WorksCorrectly()
        {
            var objectPrinter = ObjectPrinter.For<Dictionary<int, Tree<int>>>();
            var dict = new Dictionary<int, Tree<int>>
            {
                {0, new Tree<int>()},
                { 1, new Tree<int> {Left = 42, Right = 24}}
            };

            var expected = string.Join(Environment.NewLine, new[]
            {
                "Dictionary`2",
                "\tElement 0 = KeyValuePair`2",
                "\t\tKey = 0",
                "\t\tValue = Tree`1",
                "\t\t\tLeft = 0",
                "\t\t\tRight = 0",
                "\tElement 1 = KeyValuePair`2",
                "\t\tKey = 1",
                "\t\tValue = Tree`1",
                "\t\t\tLeft = 42",
                $"\t\t\tRight = 24{Environment.NewLine}",
            });

            objectPrinter
                .PrintToString(dict)
                .Should()
                .BeEquivalentTo(expected);
        }

        [Test]
        public void ObjectPrinter_OnCollectionOfObjects_WorksCorrectly()
        {
            var objectPrinter = ObjectPrinter.For<Player<Player<int>>>();
            var playerWithIntScores = new Player<int> {Name = "Bob", Scores = new List<int> {3, 2, 1}};
            var player = new Player<Player<int>> {Name = "Sasha", Scores = new List<Player<int>> {playerWithIntScores}};

            var expected = string.Join(Environment.NewLine, new[]
            {
                "Player`1",
                "\tName = Sasha",
                "\tScores = List`1",
                "\t\tElement 0 = Player`1",
                "\t\t\tName = Bob",
                "\t\t\tScores = List`1",
                "\t\t\t\tElement 0 = 3",
                "\t\t\t\tElement 1 = 2",
                $"\t\t\t\tElement 2 = 1{Environment.NewLine}",
            });

            objectPrinter
                .PrintToString(player)
                .Should()
                .BeEquivalentTo(expected);
        }

        [Test]
        public void ObjectPrinter_OnRecursiveLinks_DoesntExcludesSameObjects()
        {
            var objectPrinter = ObjectPrinter.For<Tree<Person>>();
            var person = new Person {Name = "Bob"};
            var tree = new Tree<Person> {Left = person, Right = person};

            var expected = string.Join(Environment.NewLine, new[]
            {
                "Tree`1",
                "\tLeft = Person",
                "\t\tId = Guid",
                "\t\tName = Bob",
                "\t\tHeight = 0",
                "\t\tAge = 0",
                "\tRight = Person",
                "\t\tId = Guid",
                "\t\tName = Bob",
                "\t\tHeight = 0",
                $"\t\tAge = 0{Environment.NewLine}",
            });

            objectPrinter
                .PrintToString(tree)
                .Should()
                .BeEquivalentTo(expected);
        }

        [Test]
        public void ObjectPrinter_OnFieldsWithProperties_WorksCorrectly()
        {
            var objectPrinter = ObjectPrinter.For<FooBar>();
            var numbers = new List<int> {1, 2};
            var foobar1 = new FooBar {Name = "foobar1", Numbers = numbers, Value =  10};
            var foobar2 = new FooBar {Name = "foobar2", Numbers = numbers, Parent = foobar1, Value = 20};
            foobar1.Parent = foobar1;

            var expected = string.Join(Environment.NewLine, new[]
            {
                "FooBar",
                "\tName = foobar2",
                "\tValue = 20",
                "\tNumbers = List`1",
                "\t\tElement 0 = 1",
                "\t\tElement 1 = 2",
                "\tParent = FooBar",
                "\t\tName = foobar1",
                "\t\tValue = 10",
                "\t\tNumbers = List`1",
                "\t\t\tElement 0 = 1",
                "\t\t\tElement 1 = 2",
                "\t\tParent = Circular"
            });

            objectPrinter
                .PrintToString(foobar2)
                .Should()
                .BeEquivalentTo(expected);
        }

        [Test]
        public void ObjectPrinter_OnPropertyWithNull_WorksCorrectly()
        {
            var objectPrinter = ObjectPrinter.For<Person>();;
            var person = new Person();

            var expected = string.Join(Environment.NewLine, new[]
            {
                "Person",
                "\tId = Guid",
                "\tName = null",
                "\tHeight = 0",
                $"\tAge = 0{Environment.NewLine}",
            });

            objectPrinter
                .PrintToString(person)
                .Should()
                .BeEquivalentTo(expected);
        }

        [Test]
        public void ObjectPrinter_OnInfiniteIEnumerable_DoesntThrowsException()
        {
            IEnumerable<long> PositiveNumbers()
            {
                long x = 1;
                while (true) { 
                    yield return x++;
                }
            }

            Action a = () => PositiveNumbers().PrintToString();

            a.Should().NotThrow();
        }
    }
}