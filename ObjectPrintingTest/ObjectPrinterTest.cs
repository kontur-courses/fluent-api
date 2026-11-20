using FluentAssertions;
using ObjectPrinterTest.SerializationClasses;
using ObjectPrinting;
using ObjectPrinting.Methods;
using ObjectPrintingTest.SerializationClasses;
using System.Globalization;

namespace ObjectPrinterTest
{
    public partial class ObjectPrinterTest
    {

        private static readonly string NL = Environment.NewLine;

        [Test]
        public void Serialize_PersonExcludingProperty_ReturnAsIs()
        {
            var person = new Person()
            { Name = "Alex", Age = 19, Height = 170 };

            var printer = ObjectPrinter.For<Person>()
                .Excluding<int>()
                .Excluding<double>();

            printer.PrintToString(person).Trim().Should().Be("Person\r\n\tName = Alex");

        }

        [Test]
        public void Serialize_RecursiveCall_ReturnStringException()
        {
            var linkedList = new MyLinkedList();

            linkedList.Value = 3;
            linkedList.Next = linkedList;

            var printer = ObjectPrinter.For<MyLinkedList>();

            printer.PrintToString(linkedList).Should().Be("MyLinkedList\r\n\tValue = 3\r\n\tNext = MyLinkedList (cycle)\r\n");

        }

        [Test]
        public void Serialize_List_ReturnListSerialize()
        {
            var person1 = new Person() { Name = "AAA", Age = 12 };
            var person2 = new Person() { Name = "BBB", Age = 13 };

            var personList = new List<Person>() { person1, person2 };

            var printer = ObjectPrinter.For<List<Person>>();

            var excepted = Lines(["List`1", 
                "\t[0] = Person", 
                    "\t\tName = AAA", "\t\tHeight = 0", "\t\tAge = 12",
                "\t[1] = Person", 
                    "\t\tName = BBB", "\t\tHeight = 0", "\t\tAge = 13"]);

            printer.PrintToString(personList).Should().Be(excepted);
        }

        [Test]
        public void Serialize_Array_ReturnArraySerialize()
        {
            var person1 = new Person() { Name = "AAA", Age = 12 };
            var person2 = new Person() { Name = "BBB", Age = 13 };

            var personArray = new Person[] { person1, person2 };

            var printer = ObjectPrinter.For<Person[]>();

            var excepted = Lines(["Person[]",
                "\t[0] = Person", 
                    "\t\tName = AAA", "\t\tHeight = 0", "\t\tAge = 12",
                "\t[1] = Person",
                    "\t\tName = BBB", "\t\tHeight = 0", "\t\tAge = 13"]);

            printer.PrintToString(personArray).Should().Be(excepted);

        }


        [Test]
        public void Serialize_Dictionary_ReturnDictionarySerialize()
        {
            var person1 = new Person() { Name = "AAA", Age = 12 };
            var person2 = new Person() { Name = "BBB", Age = 13 };

            var personArray = new Dictionary<Person, Person>();
            personArray[person2] = person1;

            var printer = ObjectPrinter.For< Dictionary<Person, Person>> ();

            var excepted = Lines(["Dictionary`2",
                "\t[Person",
                    "\t\tName = BBB", "\t\tHeight = 0", "\t\tAge = 13] = Person",
                    "\t\tName = AAA", "\t\tHeight = 0", "\t\tAge = 12"]);

            printer.PrintToString(personArray).Should().Be(excepted);
        }

        [Test]
        public void Printing_TypeUsing_ChangesAllMembersOfType()
        {
            var person = new Person { Name = "Alex", Age = 255 };

            var result = ObjectPrinter.For<Person>()
                .Printing<int>().Using(i => i.ToString("X"))
                .PrintToString(person);

            result.Should().Contain("Age = FF" + NL);
        }

        [Test]
        public void Printing_Culture_UsesGivenDecimalSeparator()
        {
            var comma = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            comma.NumberFormat.NumberDecimalSeparator = ",";

            var result = ObjectPrinter.For<Person>()
                .Printing<double>().Using(comma)
                .PrintToString(new Person { Height = 1.5 });

            result.Should().Contain("Height = 1,5");
        }

        [Test]
        public void Printing_MemberUsing_ChangesOnlyThatMember()
        {
            var point = new Point { X = 1, Y = 2 };   

            var result = ObjectPrinter.For<Point>()
                .Printing(p => p.X).Using(x => "x:" + x)
                .PrintToString(point);

            result.Should().Contain("X = x:1").And.Contain("Y = 2");
        }

        [Test]
        public void Printing_MemberUsing_HasPriorityOverTypeUsing()
        {
            var result = ObjectPrinter.For<Point>()
                .Printing<int>().Using(i => "type")
                .Printing(p => p.X).Using(i => "member")
                .PrintToString(new Point { X = 1, Y = 2 });

            result.Should().Contain("X = member").And.Contain("Y = type");
        }

        [Test]
        public void TrimmedToLength_TrimsLongString()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing(p => p.Name).TrimmedToLength(2)
                .PrintToString(new Person { Name = "Alex" });

            result.Should().Contain("Name = Al" + NL);
        }

        [Test]
        public void Excluding_Member_RemovesOnlyThatMember()
        {
            var result = ObjectPrinter.For<Person>()
                .Excluding(p => p.Age)
                .PrintToString(new Person { Name = "Alex", Age = 19 });

            result.Should().NotContain("Age").And.Contain("Name = Alex");
        }

        [Test]
        public void Serialize_SameStringInTwoMembers_IsNotCycle()
        {
            var result = ObjectPrinter.For<Pair>()
                .PrintToString(new Pair { A = "same", B = "same" });

            result.Should().NotContain("cycle");
        }

        [Test]
        public void Serialize_SameObjectInTwoBranches_IsNotCycle()
        {
            var shared = new Person { Name = "Alex" };
            var list = new List<Person> { shared, shared };

            ObjectPrinter.For<List<Person>>().PrintToString(list)
                .Should().NotContain("cycle");
        }

        [Test]
        public void Serialize_NullMember_PrintsNull()
        {
            ObjectPrinter.For<Person>().PrintToString(new Person { Name = null })
                .Should().Contain("Name = null" + NL);
        }

        [Test]
        public void Serialize_PublicField_IsPrinted()
        {
            var result = ObjectPrinter.For<WithField>()
                .PrintToString(new WithField { Value = 7 });

            result.Should().Contain("Value = 7");
        }

        [Test]
        public void Excluding_InheritedMember_RemovesIt()
        {
            var result = ObjectPrinter.For<Derived>()
                .Excluding(p => p.Name)
                .PrintToString(new Derived { Name = "Alex", Age = 19 });

            result.Should().NotContain("Name").And.Contain("Age = 19");
        }

        [Test]
        public void Printing_InheritedMember_AppliesFormater()
        {
            var result = ObjectPrinter.For<Derived>()
                .Printing(p => p.Name).Using(n => "<" + n + ">")
                .PrintToString(new Derived { Name = "Alex" });

            result.Should().Contain("Name = <Alex>");
        }

        [Test]
        public void Excluding_Type_ExcludesMemberWithMatchingRuntimeType()
        {
            var result = ObjectPrinter.For<Box>()
                .Excluding<int>()
                .PrintToString(new Box { Value = 42 });

            result.Should().NotContain("Value");
        }

        [Test]
        public void Excluding_Type_DoesNotExcludeMemberWithOtherRuntimeType()
        {
            var result = ObjectPrinter.For<Box>()
                .Excluding<int>()
                .PrintToString(new Box { Value = "text" });

            result.Should().Contain("Value = text");
        }

        [Test]
        public void Serialize_PropertiesWithoutPublicGetter_AreSkipped()
        {
            var result = ObjectPrinter.For<Hidden>().PrintToString(new Hidden());

            result.Should().Contain("Visible = 1")
                .And.NotContain("WriteOnly")
                .And.NotContain("PrivateGet");
        }


        private static string Lines(params string[] lines)
            => string.Join(NL, lines) + NL;

    }
}

    