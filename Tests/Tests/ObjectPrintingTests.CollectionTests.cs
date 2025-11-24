    using System.Collections;
    using System.Collections.ObjectModel;
    using FluentAssertions;
    using ObjectPrinting;
    using Tests.TestEntities;

    namespace Tests.Tests;

    public partial class ObjectPrintingTests
    {
        [TestFixture]
        public class CollectionTests
        {
            [Test]
            public void PrintToString_Array_ShouldPrintEachElement()
            {
                var data = new[] { 1, 2, 3 };
                var printer = ObjectPrinter.For<int[]>().Create();
                var result = printer.PrintToString(data);
                
                var expected = $"""
                                [
                                {'\t'}1
                                {'\t'}2
                                {'\t'}3
                                ]

                                """;
                
                result.Should().Be(expected);
            }

            [Test]
            public void PrintToString_ArrayOfObjects_ShouldSerializeEachElement()
            {
                var data = new[]
                {
                    new CollectionTestsClass { Name = "Alice", Age = 25 },
                    new CollectionTestsClass { Name = "Bob", Age = 30 }
                };
                var printer = ObjectPrinter.For<CollectionTestsClass[]>().Create();
                var result = printer.PrintToString(data);
                
                var expected = $"""
                                [
                                {'\t'}CollectionTestsClass
                                {'\t'}{'\t'}Name = Alice
                                {'\t'}{'\t'}Age = 25
                                {'\t'}{'\t'}Friends = [
                                {'\t'}{'\t'}]
                                {'\t'}CollectionTestsClass
                                {'\t'}{'\t'}Name = Bob
                                {'\t'}{'\t'}Age = 30
                                {'\t'}{'\t'}Friends = [
                                {'\t'}{'\t'}]
                                ]

                                """;

                result.Should().Be(expected);
            }
            
            [Test]
            public void PrintToString_Dictionary_ShouldPrintKeysAndValues()
            {
                var dict = new Dictionary<int, string>
                {
                    [1] = "1",
                    [2] = "2"
                };
                var printer = ObjectPrinter.For<Dictionary<int, string>>().Create();
                var result = printer.PrintToString(dict);
                
                var expected = $$"""
                                 {
                                 {{'\t'}}[1] = 1
                                 {{'\t'}}[2] = 2
                                 }

                                 """;

                result.Should().Be(expected);
            }


            [Test]
            public void PrintToString_DictionaryWithComplexObjects_ShouldSerializeKeysAndValues()
            {
                var dict = new Dictionary<CollectionTestsClass, Department>
                {
                    [new CollectionTestsClass { Name = "Manager" }] = new() { Name = "HR" },
                    [new CollectionTestsClass { Name = "Developer" }] = new() { Name = "IT" }
                };

                var printer = ObjectPrinter
                    .For<Dictionary<CollectionTestsClass, Department>>()
                    .Create();
                var result = printer.PrintToString(dict);
                
                var expected = $$"""
                                 {
                                 {{'\t'}}[CollectionTestsClass
                                 {{'\t'}}{{'\t'}}Name = Manager
                                 {{'\t'}}{{'\t'}}Age = 0
                                 {{'\t'}}{{'\t'}}Friends = [
                                 {{'\t'}}{{'\t'}}]] = Department
                                 {{'\t'}}{{'\t'}}Name = HR
                                 {{'\t'}}[CollectionTestsClass
                                 {{'\t'}}{{'\t'}}Name = Developer
                                 {{'\t'}}{{'\t'}}Age = 0
                                 {{'\t'}}{{'\t'}}Friends = [
                                 {{'\t'}}{{'\t'}}]] = Department
                                 {{'\t'}}{{'\t'}}Name = IT
                                 }

                                 """;
                
                result.Should().Be(expected);
            }
            

            [Test]
            public void PrintToString_NullCollection_ShouldShowNull()
            {
                var data = new CollectionTestsClass { Name = "Test", Friends = null };
                var printer = ObjectPrinter
                    .For<CollectionTestsClass>()
                    .Create();
                var result = printer.PrintToString(data);
                
                var expected = $"""
                                CollectionTestsClass
                                {'\t'}Name = Test
                                {'\t'}Age = 0
                                {'\t'}Friends = null
                                
                                """;
                

                result.Should().Be(expected);
            }
            
            private static IEnumerable<TestCaseData> EmptyCollectionsTestCases
            {
                get
                {
                    yield return new TestCaseData(Array.Empty<int>())
                        .SetName("EmptyArray_ShouldShowEmptyArrayStructure");
                    
                    yield return new TestCaseData(new List<string>())
                        .SetName("EmptyList_ShouldShowEmptyListStructure");
                    
                    yield return new TestCaseData(new HashSet<double>())
                        .SetName("EmptyHashSet_ShouldShowEmptyHashSetStructure");
                    
                    yield return new TestCaseData(new Collection<bool>())
                        .SetName("EmptyCollection_ShouldShowEmptyCollectionStructure");
                }
            }
            
            [Test]
            [TestCaseSource(nameof(EmptyCollectionsTestCases))]
            public void PrintToString_EmptyCollections_ShouldShowEmptyStructure(
                IEnumerable emptyCollection)
            {
                var printer = ObjectPrinter.For<IEnumerable>().Create();
                var result = printer.PrintToString(emptyCollection);
                
                var expected = $"[{Environment.NewLine}]{Environment.NewLine}";
                
                result.Should().Be(expected);
            }

            [Test]
            public void PrintToString_EmptyDictionary_ShouldShowEmptyStructure()
            {
                var printer  = ObjectPrinter.For<Dictionary<object, object>>().Create();
                var result = printer.PrintToString(new Dictionary<object, object>());
                
                var expected = $"{{{Environment.NewLine}}}{Environment.NewLine}";
                result.Should().Be(expected);
            }
        }
    }