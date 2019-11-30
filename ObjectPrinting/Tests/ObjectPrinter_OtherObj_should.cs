using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinter_OtherObj_should
    {
        [Test]
        public void PrintToString_WhenIEnum()
        {
            var result = new[] {1, 2, 3, 4};
            foreach (var num in result)
            {
                result.PrintToString().Should().Contain(num.ToString());
            }
        }

        [Test]
        public void PrintToString_WhenNestedClass()
        {
            var parent = new Person {Name = "Parent"};
            var children = new PersonWithParent {Name = "Children", Parent = parent};
            children.PrintToString().Should().Contain(parent.Name).And.Contain(children.Name);
        }
        
        [Test]
        public void PrintToString_WhenCircleRefClass()
        {
            var parent = new PersonWithParent {Name = "Parent"};
            var children = new PersonWithParent {Name = "Children", Parent = parent};
            parent.Parent = children;
            children.PrintToString().Should().Contain("circle reference");
        }
    }

    internal class PersonWithParent : Person
    {
        public Person Parent { get; set; }
    }
}