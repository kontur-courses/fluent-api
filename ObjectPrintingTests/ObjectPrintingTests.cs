using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Infrastructure;
using ObjectPrintingTests.Mocks;

namespace ObjectPrintingTests
{
    public class Tests
    {
        [Test]
        [TestCaseSource(nameof(GetDocumentedTestCaseData))]
        public void PrintToStringTests<T>(PrintingConfig<T> printingConfig, T toPrint, string expected)
        {
            var actual = printingConfig.PrintToString(toPrint);

            actual.Should().Be(expected);
        }

        private static IEnumerable<TestCaseData> GetDocumentedTestCaseData()
        {
            List<(string testName, object config, object toPrint, string expectedRaw)> testData =
                new List<(string testName, object config, object toPrint, string expectedRaw)>
                {
                    ("Cycle reference",
                        ObjectPrinter.For<Node>(),
                        GetCycleReferencedObject(),
                        "Node\n\tNext = Node\n\t\tNext = [cycle]\n"),

                    ("With String Shortening",
                        ObjectPrinter.For<Text>()
                            .Printing<string>()
                            .TrimmedToLength(2),
                        new Text {String = "1234"},
                        "Text\n\tString = 12\n"),

                    ("Custom Property",
                        ObjectPrinter.For<Household>()
                            .Printing(p => p.Property)
                            .Using(p => "custom"),
                        new Household {Property = null},
                        "Household\n\tProperty = custom\n"),

                    ("Culture setting",
                        ObjectPrinter.For<Number>()
                            .Printing<double>()
                            .Using(CultureInfo.GetCultureInfo("sv-SE")),
                        new Number() {Double = 1.1},
                        "Number\n\tDouble = 1,1\n"),

                    ("Selected field formatted",
                        ObjectPrinter.For<Index>()
                            .Printing(p => p.Integer)
                            .Using(i => "i"),
                        new Index(),
                        "Index\n\tInteger = i\n"),

                    ("Selected type formatted",
                        ObjectPrinter.For<Index>()
                            .Printing<int>()
                            .Using(i => "i"),
                        new Index(),
                        "Index\n\tInteger = i\n"),

                    ("Excluding By Type",
                        ObjectPrinter.For<Person>()
                            .Excluding<int>(),
                        new Person {Name = "name", Age = 10},
                        "Person\n\tName = name\n"),

                    ("Excluding By Field",
                        ObjectPrinter.For<Person>()
                            .Excluding(p => p.Age),
                        new Person {Name = "name", Age = 10},
                        "Person\n\tName = name\n"),

                    ("Array of Persons",
                        ObjectPrinter.For<Person[]>(),
                        new[]
                        {
                            new Person {Name = "A", Age = 1},
                            new Person {Name = "B", Age = 2},
                            new Person {Name = "C", Age = 3},
                        },
                        "Person[]\n\tPerson\n\t\tName = A\n\t\tAge = 1\n\tPerson\n\t\tName = B\n\t\tAge = 2\n\tPerson\n\t\tName = C\n\t\tAge = 3\n"),

                    ("List of Persons",
                        ObjectPrinter.For<List<Person>>(),
                        new List<Person>
                        {
                            new Person {Name = "A", Age = 1},
                            new Person {Name = "B", Age = 2},
                            new Person {Name = "C", Age = 3},
                        },
                        "List`1\n\tPerson\n\t\tName = A\n\t\tAge = 1\n\tPerson\n\t\tName = B\n\t\tAge = 2\n\tPerson\n\t\tName = C\n\t\tAge = 3\n"),

                    ("Dictionary with int key, Person value",
                        ObjectPrinter.For<Dictionary<int, Person>>(),
                        new Dictionary<int, Person>
                        {
                            {1, new Person {Name = "A", Age = 1}},
                            {2, new Person {Name = "B", Age = 2}},
                            {3, new Person {Name = "C", Age = 3}},
                        },
                        "Dictionary`2\n\tKeyValuePair`2\n\t\tKey = 1\n\t\tValue = Person\n\t\t\tName = A\n\t\t\tAge = 1\n\tKeyValuePair`2\n\t\tKey = 2\n\t\tValue = Person\n\t\t\tName = B\n\t\t\tAge = 2\n\tKeyValuePair`2\n\t\tKey = 3\n\t\tValue = Person\n\t\t\tName = C\n\t\t\tAge = 3\n"),
                };

            foreach (var data in testData)
            {
                yield return new TestCaseData(
                        data.config,
                        data.toPrint,
                        GetSystemIndependent(data.expectedRaw))
                    {TestName = data.testName};
            }
        }

        private static string GetSystemIndependent(string text)
        {
            return text.Replace("\n", Environment.NewLine);
        }

        private static Node GetCycleReferencedObject()
        {
            var node1 = new Node();
            var node2 = new Node {Next = node1};
            node1.Next = node2;
            return node1;
        }
    }
}