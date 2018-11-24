using System;
using System.Collections.Generic;
using System.Globalization;
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
                "\tName = ob",
                "\tHeight = 0",
                $"\tAge = 100{Environment.NewLine}",
            });

            objectPrinter
                .Printing(p => p.Name)
                .Trim(1)
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
        public void ObjectPrinter_OnRecursiveLinks_ThrowsInfiniteRecursionException()
        {
            var objectPrinter = ObjectPrinter.For<Node>();
            var parent = new Node() {Value = 1};
            var node = new Node {Value = 1, Parent = parent};
            parent.Parent = node;

            Action action = () => objectPrinter
                .PrintToString(node);

            action.Should()
                .Throw<InfiniteRecursionException>();
        }

        [Test]
        public void ObjectPrinter_OnCollection_WorksCorrectly()
        {
            var objectPrinter = ObjectPrinter.For<Player>();
            var player = new Player {Name = "Sasha", Scores = new List<int> {3, 2, 1}};

            var expected = string.Join(Environment.NewLine, new[]
            {
                "Player",
                "\tName = Sasha",
                "\tScores = ICollection`1",
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
    }
}