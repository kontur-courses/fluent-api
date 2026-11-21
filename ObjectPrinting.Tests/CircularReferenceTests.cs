using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class CircularReferenceTests
    {
        [Test]
        public void PrintToString_HandlesSelfReference()
        {
            var person = new PersonNode
            {
                Name = "Alex"
            };

            person.Friend = person;

            var result = ObjectPrinter.For<PersonNode>()
                .PrintToString(person);

            var expected =
                "PersonNode" + PrintingTestFormat.NewLine +
                "\tName = Alex" + PrintingTestFormat.NewLine +
                "\tFriend = " + PrintingTestFormat.CircularReferenceMarker +
                PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_HandlesMutualReferences()
        {
            var first = new PersonNode
            {
                Name = "Alex"
            };

            var second = new PersonNode
            {
                Name = "Bob"
            };

            first.Friend = second;
            second.Friend = first;

            var result = ObjectPrinter.For<PersonNode>()
                .PrintToString(first);

            var expected =
                "PersonNode" + PrintingTestFormat.NewLine +
                "\tName = Alex" + PrintingTestFormat.NewLine +
                "\tFriend = PersonNode" + PrintingTestFormat.NewLine +
                "\t\tName = Bob" + PrintingTestFormat.NewLine +
                "\t\tFriend = " + PrintingTestFormat.CircularReferenceMarker +
                PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_SerializesSharedReferenceInDifferentBranches()
        {
            var shared = new PersonNode
            {
                Name = "Shared"
            };

            var root = new PersonTree
            {
                FirstFriend = shared,
                SecondFriend = shared
            };

            var result = ObjectPrinter.For<PersonTree>()
                .PrintToString(root);

            var expected =
                "PersonTree" + PrintingTestFormat.NewLine +
                "\tFirstFriend = PersonNode" + PrintingTestFormat.NewLine +
                "\t\tName = Shared" + PrintingTestFormat.NewLine +
                "\t\tFriend = null" + PrintingTestFormat.NewLine +
                "\tSecondFriend = PersonNode" + PrintingTestFormat.NewLine +
                "\t\tName = Shared" + PrintingTestFormat.NewLine +
                "\t\tFriend = null" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void PrintToString_UsesReferenceEqualityInsteadOfObjectEquality()
        {
            var first = new EqualPersonNode
            {
                Name = "First"
            };

            var second = new EqualPersonNode
            {
                Name = "Second"
            };

            first.Friend = second;

            var result = ObjectPrinter.For<EqualPersonNode>()
                .PrintToString(first);

            var expected =
                "EqualPersonNode" + PrintingTestFormat.NewLine +
                "\tName = First" + PrintingTestFormat.NewLine +
                "\tFriend = EqualPersonNode" + PrintingTestFormat.NewLine +
                "\t\tName = Second" + PrintingTestFormat.NewLine +
                "\t\tFriend = null" + PrintingTestFormat.NewLine;

            Assert.That(result, Is.EqualTo(expected));
        }

        private sealed class PersonNode
        {
            public string Name { get; set; } = string.Empty;

            public PersonNode? Friend { get; set; }
        }

        private sealed class PersonTree
        {
            public PersonNode? FirstFriend { get; set; }

            public PersonNode? SecondFriend { get; set; }
        }

        private sealed class EqualPersonNode
        {
            public string Name { get; set; } = string.Empty;

            public EqualPersonNode? Friend { get; set; }

            public override bool Equals(object? obj)
            {
                return obj is EqualPersonNode;
            }

            public override int GetHashCode()
            {
                return 0;
            }
        }
    }
}