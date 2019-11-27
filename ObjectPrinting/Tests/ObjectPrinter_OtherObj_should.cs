using System.Collections;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class ObjectPrinter_OtherObj_should
    {
        [Test]
        public void PrintToString_WhenIEnum()
        {
            IEnumerable result = new int[] {1, 2, 3, 4};
            result.PrintToString().Should().Be("Int32[]\r\n	1\r\n	2\r\n	3\r\n	4\r\n");
        }

        [Test]
        public void PrintToString_WhenNestedClass()
        {
            var parent = new Person() {Name = "Parent"};
            var children = new PersonWithParent() {Name = "Children", Parent = parent};
            children.PrintToString().Should()
                .Be(
                    "PersonWithParent\r\n	Parent = Person\r\n		Id = Guid\r\n		Name = Parent\r\n		" +
                    "Height = 0\r\n		Age = 0\r\n	Id = Guid\r\n	Name = Children\r\n	Height = 0\r\n	Age = 0\r\n");
        }
    }

    internal class PersonWithParent : Person
    {
        public Person Parent { get; set; }
    }
}