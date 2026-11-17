using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class CircularReferenceTests
    {
        [Test]
        public void PrintToString_HandlesSelfReference()
        {
            var person = new PersonNode { Name = "Alex" };
            person.Friend = person;

            var result = ObjectPrinter.For<PersonNode>()
                .PrintToString(person);

            Assert.That(result, Does.Contain("Friend = " + PrintingFormat.CircularReferenceMarker));
        }

        [Test]
        public void PrintToString_HandlesMutualReferences()
        {
            var first = new PersonNode { Name = "Alex" };
            var second = new PersonNode { Name = "Bob" };

            first.Friend = second;
            second.Friend = first;

            var result = ObjectPrinter.For<PersonNode>()
                .PrintToString(first);

            Assert.That(result, Does.Contain("Name = Bob"));
            Assert.That(result, Does.Contain(PrintingFormat.CircularReferenceMarker));
        }

        [Test]
        public void PrintToString_SerializesSharedReferenceInDifferentBranches()
        {
            var shared = new PersonNode { Name = "Shared" };

            var root = new PersonTree
            {
                FirstFriend = shared,
                SecondFriend = shared
            };

            var result = ObjectPrinter.For<PersonTree>()
                .PrintToString(root);

            var occurrences = result.Split("Name = Shared").Length - 1;

            Assert.That(occurrences, Is.EqualTo(2));
            Assert.That(result, Does.Not.Contain(PrintingFormat.CircularReferenceMarker));
        }

        [Test]
        public void PrintToString_UsesReferenceEqualityInsteadOfObjectEquality()
        {
            var first = new EqualPersonNode { Name = "First" };
            var second = new EqualPersonNode { Name = "Second" };

            first.Friend = second;

            var result = ObjectPrinter.For<EqualPersonNode>()
                .PrintToString(first);

            Assert.That(result, Does.Contain("Name = Second"));
            Assert.That(result, Does.Not.Contain(PrintingFormat.CircularReferenceMarker));
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