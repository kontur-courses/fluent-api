using FluentAssertions;
using NUnit.Framework;
using System.Collections.Generic;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        [Test]
        public void ExcludingProperty_WhenExcluding()
        {
            var person = new ClassForTests("Ivan", age: 123);
            var printer = ObjectPrinter.For<ClassForTests>()
                .Excluding(p => p.Age);

            var s = printer.PrintToString(person);

            s.Should().NotContain("Age");
        }

        [Test]
        public void SerializingFields_IfObjectHasIt()
        {
            var person = new ClassForTests("Ivan", age: 123);
            var printer = ObjectPrinter.For<ClassForTests>();

            var s = printer.PrintToString(person);

            s.Should().Contain("Height");
        }


        [Test]
        public void ExcludingAllPropertiesOfType_WhenExcluding()
        {
            var person = new ClassForTests("Ivan", "Kogut", 19);
            var printer = ObjectPrinter.For<ClassForTests>()
                .Excluding<string>();

            var s = printer.PrintToString(person);

            s.Should().NotContain("FirstName").And.NotContain("Surname");
        }

        [Test]
        public void CutStringProperties_WhenCutPrefix()
        {
            var person1 = new ClassForTests("Ivan", "Kogut", 19);
            var person2 = new ClassForTests("Alex", "Smelov", 19);
            var printer = ObjectPrinter.For<ClassForTests>()
                .Serializing(p => p.FirstName).CutPrefix(2)
                .Serializing(p => p.Surname).CutPrefix(3);

            var sIvan = printer.PrintToString(person1);
            var sAlex = printer.PrintToString(person2);

            sIvan.Should().Contain("Iv").And.NotContain("Iva");
            sIvan.Should().Contain("Kog").And.NotContain("Kogu");
            sAlex.Should().Contain("Al").And.NotContain("Ale");
            sAlex.Should().Contain("Sme").And.NotContain("Smel");
        }

        [Test]
        public void SerializeTypeInOwnWay_WhenSerializing()
        {
            var person1 = new ClassForTests("Ivan", "Kogut", 19);
            var person2 = new ClassForTests("Alex", "Smelov", 19);
            var printer = ObjectPrinter.For<ClassForTests>()
                .Serializing<string>().Using(x => "Vitya");

            var sIvan = printer.PrintToString(person1);
            var sAlex = printer.PrintToString(person2);

            sIvan.Should().Contain("Vitya").And.NotContain("Ivan").And.NotContain("Kogut");
            sAlex.Should().Contain("Vitya").And.NotContain("Alex").And.NotContain("Smelov");
        }

        [Test]
        public void SerializingFieldsInWayOfType_WhenSerializingItsType()
        {
            var person = new ClassForTests("Ivan", age: 123);
            var printer = ObjectPrinter.For<ClassForTests>()
                .Serializing<double>().Using(x => "Long");

            var s = printer.PrintToString(person);

            s.Should().Contain("Long");
        }

        [Test]
        public void SerializePropertyInOwnWay_WhenSerializing()
        {
            var person1 = new ClassForTests("Ivan", "Kogut", 19);
            var person2 = new ClassForTests("Alex", "Smelov", 19);
            var printer = ObjectPrinter.For<ClassForTests>()
                .Serializing(p => p.Age).Using(a => "A lot");
            var sIvan = printer.PrintToString(person1);
            var sAlex = printer.PrintToString(person2);

            sIvan.Should().NotContain("19").And.Contain("A lot");
            sAlex.Should().NotContain("19").And.Contain("A lot");
        }

        [Test]
        public void SerializeInPropertyWayDespiteTypeWay_WhenSerializingForPropertyAndType()
        {
            var person1 = new ClassForTests("Ivan", "Kogut", 19);
            var person2 = new ClassForTests("Alex", "Smelov", 19);
            var printer = ObjectPrinter.For<ClassForTests>()
                .Serializing<int>().Using(p => "Bullshit")
                .Serializing(p => p.Age).Using(p => "A lot");

            var sIvan = printer.PrintToString(person1);
            var sAlex = printer.PrintToString(person2);

            sIvan.Should().NotContain("Bullshit").And.Contain("A lot");
            sAlex.Should().NotContain("Bullshit").And.Contain("A lot");
        }

        [Test]
        public void NotFall_IfObjectHasRecursionLinks()
        {
            var person1 = new ClassForTests("Ivan", "Kogut", 19);
            var person2 = new ClassForTests("Alex", "Smelov", 19);
            person1.Friend = person2;
            person2.Friend = person1;
            var printer = ObjectPrinter.For<ClassForTests>();

            var s = printer.PrintToString(person1);

            s.IndexOf("Ivan").Should().Be(s.LastIndexOf("Ivan"));
            s.Should().Contain("<cyclic link is detected>");
        }

        [Test]
        public void SerializeArray_IfArray()
        {
            var array = new int[]
            {
                1,
                2,
                3
            };
            var printer = ObjectPrinter.For<int[]>();

            var s = printer.PrintToString(array);

            s.Should().Contain("1").And.Contain("2").And.Contain("3");
        }

        [Test]
        public void SerializeList_IfList()
        {
            var list = new List<string>
            {
                "123",
                "abc",
                "yyy"
            };
            var printer = ObjectPrinter.For<List<string>>();

            var s = printer.PrintToString(list);

            s.Should().Contain("123").And.Contain("abc").And.Contain("yyy");
        }

        [Test]
        public void SerializeNestingList_IfNestingList()
        {
            var list = new List<List<string>>
            {
                new List<string>()
                {
                    "ab", "cd"
                },
                new List<string>()
                {
                    "ef"
                },
                new List<string>()
                {
                    "gh"
                }
            };
            var printer = ObjectPrinter.For<List<List<string>>>();

            var s = printer.PrintToString(list);

            s.Should().Contain("ab").And.Contain("cd").And.Contain("ef").And.Contain("gh");
        }

        [Test]
        public void SerializeDict_IfDict()
        {
            var dict = new Dictionary<int, string>()
            {
                { 1, "A"},
                { 2, "B"},
                { 3, "Friend"}
            };
            var printer = ObjectPrinter.For<Dictionary<int, string>>();

            var s = printer.PrintToString(dict);

            s.Should().Contain("A").And.Contain("B").And.Contain("Friend");
            s.Should().Contain("1").And.Contain("2").And.Contain("3");
        }
    }
}