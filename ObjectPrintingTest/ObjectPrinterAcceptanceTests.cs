using System.Globalization;
using FluentAssertions;
using ObjectPrinting;
using ObjectPrintingTest.TestTypes;

namespace ObjectPrintingTest
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private IBaseConfig<Person> sut;

        [SetUp]
        public void SetUp()
        {
            sut = ObjectPrinter.For<Person>();
        }

        [Test]
        public void ExcludeType()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.23 };
            sut
                .Exclude<double>()
                .Exclude<int>()
                .PrintToString(person)
                .Should()
                .NotContainAll("Height", "Age");
        }

        [Test]
        public void ExcludeProperty()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            sut
                .Exclude(p => p.Name)
                .Exclude(p => p.Age)
                .PrintToString(person)
                .Should()
                .NotContainAll(new[] { "Name", "Age" });
        }

        [Test]
        public void SerializeType()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            sut
                .Printing<string>()
                .SerializeAs(s => s + s)
                .PrintToString(person)
                .Should()
                .Contain("Name = AlexAlex");
        }

        [Test]
        public void SerializeProperty()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            sut
                .Printing(p => p.Age)
                .SerializeAs(age => $"{age * 3}")
                .PrintToString(person)
                .Should()
                .Contain("Age = 57");
        }

        [Test]
        public void Truncate()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            sut
                .Printing(p => p.Name)
                .Truncate(1, 3)
                .PrintToString(person)
                .Should()
                .Contain("Name = lex");
        }

        [Test]
        public void CultureInfoForIFormattableType()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.23 };
            sut
                .PrintToString(person)
                .Should()
                .Contain("Height = 1,23");
            sut
                .Printing<double>()
                .SetCulture(CultureInfo.InvariantCulture)
                .PrintToString(person)
                .Should()
                .Contain("Height = 1.23");
        }

        [Test]
        public void CultureInfoForIFormattableProperty()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.23 };
            sut
                .PrintToString(person)
                .Should()
                .Contain("Height = 1,23");
            sut
                .Printing(p => p.Height)
                .SetCulture(CultureInfo.InvariantCulture)
                .PrintToString(person)
                .Should()
                .Contain("Height = 1.23");
        }

        [Test]
        public void PrintList()
        {
            var node = new Node(
                "A",
                new List<Node>
                {
                    new Node("B"),
                    new Node(
                        "C",
                        new List<Node>()
                        {
                            new Node("D"),
                            new Node("E")
                        }
                    )
                });
            var result = ObjectPrinter.For<Node>().PrintToString(node);
            Console.WriteLine(result);
            result
                .Should()
                .Be(
                    """
                    Node
                    	Name = A
                    	Nodes: 
                    		0: Node
                    			Name = B
                    			Nodes: <empty>
                    		1: Node
                    			Name = C
                    			Nodes: 
                    				0: Node
                    					Name = D
                    					Nodes: <empty>
                    				1: Node
                    					Name = E
                    					Nodes: <empty>

                    """);
        }

        [Test]
        public void PrintDictionaries()
        {
            var phone = new PhoneBook(
                "New York",
                new Dictionary<string, Person>()
                {
                    { "+1 (646) 555-3456", new Person() { Name = "John Doe", Age = 30 } },
                    { "+1 (646) 555-4567", new Person() { Name = "Jane Doe", Age = 30 } }
                }
            );
            var result = ObjectPrinter.For<PhoneBook>().PrintToString(phone);
            Console.WriteLine(result);
            result
                .Should()
                .Be(
                    """
                    PhoneBook
                    	Town = New York
                    	NumberToPerson: 
                    		0 element:
                    			Key: +1 (646) 555-3456
                    			Value: Person
                    				Id = 00000000-0000-0000-0000-000000000000
                    				Name = John Doe
                    				Height = 0
                    				Age = 30
                    		1 element:
                    			Key: +1 (646) 555-4567
                    			Value: Person
                    				Id = 00000000-0000-0000-0000-000000000000
                    				Name = Jane Doe
                    				Height = 0
                    				Age = 30

                    """);
        }

        [Test]
        public void Cycled()
        {
            var node = new Node("A").Add(new Node("B"));
            node.Add(node);
            var result = ObjectPrinter.For<Node>().PrintToString(node);
            Console.WriteLine(result);
            result
                .Should()
                .Be(
                    """
                    Node
                    	Name = A
                    	Nodes: 
                    		0: Node
                    			Name = B
                    			Nodes: <empty>
                    		1: cycled

                    """);
        }
    }
}