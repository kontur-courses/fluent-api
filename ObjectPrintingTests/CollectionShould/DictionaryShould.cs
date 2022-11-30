namespace ObjectPrintingTests.CollectionShould
{
    public class DictionaryShould
    {
        private string printedString;
        private PrintingConfig<Dictionary<object, object>> printer;


        [TearDown]
        public void TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Failed)
                Console.WriteLine(printedString);
        }

        public void AssertThatDictionaryContainCollection(Dictionary<object, object> dictionaryWithObjects,
            string shouldContainInResult)
        {
            printer = ObjectPrinter.For<Dictionary<object, object>>();
            printedString = printer.PrintToString(dictionaryWithObjects);
            printedString.Should().Contain(shouldContainInResult);
        }

        [Test]
        public void PrintDictionaryCollection_WhenOneLineWithoutClassesWithFields()
        {
            var dictionary = new Dictionary<object, object> { [1] = 2, [3] = 4, [5] = 6 };
            AssertThatDictionaryContainCollection(dictionary, "[[1] = 2, [3] = 4, [5] = 6]");

        }

        [Test]
        public void PrintDictionaryCollection_WhenHaveNestingAndListKeyForDictionary()
        {
            var dictionary = new Dictionary<object, object>()
                { [1] = 2, [3] = new List<int>() { 1, 2, 3 }, [new List<string> { "a,e", "something" }] = 2 };
            AssertThatDictionaryContainCollection(dictionary, "[[1] = 2, [3] = [1, 2, 3], [[a,e, something]] = 2]");
        }

        [Test]
        public void PrintDictionaryCollection_WhenHaveNestingDictionary()
        {
            var dictionary = new Dictionary<object, object>
            {
                [1] = new Dictionary<object, object>() { [1] = "edee", [2] = "rffe", [5] = "fffff" },
                [new Dictionary<int, string>() { [1] = "edee", [2] = "rffe", [5] = "fffff" }] = 1,
                [2] = 3,
                [3] = new List<int>() { 1, 2, 3 }, ["3f"] = new List<string> { "a,e", "something" }
            };
            AssertThatDictionaryContainCollection(dictionary,
                "[[1] = [[1] = edee, [2] = rffe, [5] = fffff], [[[1] = edee, [2] = rffe, [5] = fffff]] = 1, [2] = 3, [3] = [1, 2, 3], [3f] = [a,e, something]]");
        }

        [Test]
        public void PrintDictionaryCollection_WhenHaveNestingArray()
        {

            var dictionary = new Dictionary<object, object>
            {
                [2] = new[] { 1, 2, 3, 4 }, [1] = new[] { 2, 3, 5, 5, 45, 4, 54, 54 }, [55] = new[] { 1336, 1337, 1338 }
            };
            printedString = printer.PrintToString(dictionary);
            AssertThatDictionaryContainCollection(dictionary,
                "[[2] = [1, 2, 3, 4], [1] = [2, 3, 5, 5, 45, 4, 54, 54], [55] = [1336, 1337, 1338]]");
        }

        [Test]
        public void PrintDictionaryCollection_WhenHaveNestingOtherIEnumerableCollection()
        {
            var dictionary = new Dictionary<object, object>
            {
                [1] = new HashSet<int>() { 1, 2, 3, 4 }, [2] = new HashSet<int>() { 2, 3, 5, 5, 45, 4, 54, 54 },
                [3] = new HashSet<int>() { 1336, 1337, 1338 }
            };
            AssertThatDictionaryContainCollection(dictionary,
                "[[1] = [1, 2, 3, 4], [2] = [2, 3, 5, 45, 4, 54], [3] = [1336, 1337, 1338]]"); //Cant repeat([[1, 2, 3, 4], [2, 3, 5, 5, 45, 4, 54, 54], [1336, 1337, 1338]]) because HashSet 
        }

        [Test]
        public void PrintDictionaryCollection_WhenHaveClassWithFields()
        {
            var dictionary = new Dictionary<object, object> { [2] = new Person() { Age = 22, Name = "Sirgey" } };
            AssertThatDictionaryContainCollection(dictionary,
                "[[2] = Person; Name = Sirgey; Age = 22; HaveCar = False]");
        }

        [Test]
        public void PrintDictionaryCollection_WhenHaveClassesWithFields()
        {
            var dictionary = new Dictionary<object, object>
            {
                ["PetrPerson"] = new Person() { Age = 22, Name = "Sirgey" },
                ["PetrPerson2"] = new Person() { Age = 24, Height = 155, Name = "Anna" }
            };
            AssertThatDictionaryContainCollection(dictionary,
                "[[PetrPerson] = Person; Name = Sirgey; Age = 22; HaveCar = False, [PetrPerson2] = Person; Name = Anna; Height = 155; Age = 24; HaveCar = False]");
        }

        [Test]
        public void PrintDictionaryCollection_WhenHaveClassWithClassField()
        {
            var dictionary = new Dictionary<object, object>
                { [2] = new Person { Age = 22, Name = "Sirgey", Car = new Vehicle("BMW", 230, 1500, 2003) } };
            AssertThatDictionaryContainCollection(dictionary,
                "[[2] = Person; Name = Sirgey; Age = 22; HaveCar = True; Car = Vehicle; CarNumber = 0; BrandAuto = BMW; Power = 230; Weight = 1500; AgeOfCar = 19]");
        }
    }
}
