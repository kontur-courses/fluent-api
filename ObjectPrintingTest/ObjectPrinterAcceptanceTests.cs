using System.Security.Cryptography;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrintingTest;
using ObjectPrintingTest.TestTypes;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private IBaseConfig<Person> printer;
        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
        }

        [Test]
        public void ExcludeType()
        {
            var person = new Person { Name = "Alex", Age = 19};
            printer
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
            printer
                .Exclude(p => p.Name)
                .Exclude(p => p.Age)
                .PrintToString(person)
                .Should()
                .NotContainAll(new [] { "Name", "Age" });
        }
        
        [Test]
        public void SerializeType()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            printer
                .Printing<string>()
                .SerializeAs(s => s+s)
                .PrintToString(person)
                .Should()
                .Contain("Name = AlexAlex");
        }
        
        [Test]
        public void SerializeProperty()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            printer
                .Printing(p => p.Age)
                .SerializeAs(age => $"{age*3}")
                .PrintToString(person)
                .Should()
                .Contain("Age = 57");
        }
        
        [Test]
        public void Truncate()
        {
            var person = new Person { Name = "Alex", Age = 19 };
            printer
                .Printing( p => p.Name)
                .Truncate(1, 3)
                .PrintToString(person)
                .Should()
                .Contain("Name = lex");
        }
        
        [Test]
        public void CultureInfoForType()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.23};
            printer
                .Printing<double>()
                .SetCulture(System.Globalization.CultureInfo.CurrentCulture)
                .PrintToString(person)
                .Should()
                .Contain("Height = 1,23");
        }
        
        [Test]
        public void CultureInfoForProperty()
        {
            var person = new Person { Name = "Alex", Age = 19, Height = 1.23};
            printer
                .Printing(p => p.Height)
                .SetCulture(System.Globalization.CultureInfo.CurrentCulture)
                .PrintToString(person)
                .Should()
                .Contain("Height = 1,23");
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
                .Be("Node\r\n\tName = A\r\n\tNodes: \r\n\t\t1: Node\r\n\t\t\tName = B\r\n\t\t\tNodes: <empty>\r\n\t\t2: Node\r\n\t\t\tName = C\r\n\t\t\tNodes: \r\n\t\t\t\t1: Node\r\n\t\t\t\t\tName = D\r\n\t\t\t\t\tNodes: <empty>\r\n\t\t\t\t2: Node\r\n\t\t\t\t\tName = E\r\n\t\t\t\t\tNodes: <empty>\r\n");
        }
        
        [Test]
        public void PrintDictionaries()
        {
            var phone = new PhoneBook(
                "New York",
                new Dictionary<string, Person>()
                {
                    { "+1 (646) 555-3456", new Person(){Name="John Doe", Age = 30} },
                    { "+1 (646) 555-4567", new Person(){Name="Jane Doe", Age = 30} }
                }
            );
            var result = ObjectPrinter.For<PhoneBook>().PrintToString(phone);
            Console.WriteLine(result);
            result
                .Should()
                .Be("PhoneBook\r\n\tTown = New York\r\n\tNumberToPerson: \r\n\t\t1 element:\r\n\t\t\tKey: +1 (646) 555-3456\r\n\t\t\tValue: Person\r\n\t\t\t\tId = 00000000-0000-0000-0000-000000000000\r\n\t\t\t\tName = John Doe\r\n\t\t\t\tHeight = 0\r\n\t\t\t\tAge = 30\r\n\t\t2 element:\r\n\t\t\tKey: +1 (646) 555-4567\r\n\t\t\tValue: Person\r\n\t\t\t\tId = 00000000-0000-0000-0000-000000000000\r\n\t\t\t\tName = Jane Doe\r\n\t\t\t\tHeight = 0\r\n\t\t\t\tAge = 30\r\n");
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
                .Be("Node\r\n\tName = A\r\n\tNodes: \r\n\t\t1: Node\r\n\t\t\tName = B\r\n\t\t\tNodes: <empty>\r\n\t\t2: cycled\r\n");
        }
    }
}