using ObjectPrinting;
using System.Reflection;

namespace ObjectPrintingTests.CollectionShould
{
    [TestFixture]
    public class ListShould
    {
        private string printedString;
        public Vehicle VehicleCar;
        private Person simplePerson;
        private Person childPerson;
        private PrintingConfig<List<object>> printer;

        [SetUp]
        public void SetUp()
        {
            VehicleCar = new Vehicle("Audi", 230, 1400, 2005);

            simplePerson = new Person() { Age = 35, Height = 155, Id = new Guid(), Name = "Anna", Car = VehicleCar };
            childPerson = new Person()
            {
                Age = 8,
                Height = 86.34,
                Id = new Guid(),
                Name = "Seraphina",
                Parent = simplePerson
            };
            printedString = null;

        }

        [TearDown]
        public void TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Failed)
                Console.WriteLine(printedString);
        }

        public void AssertThatListContainCollection(List<object> listWithObjects, string shouldContainInResult)
        {
            printer = ObjectPrinter.For<List<object>>();
            printedString = printer.PrintToString(listWithObjects);
            printedString.Should().Contain(shouldContainInResult);
        }

        [Test]
        public void PrintListCollection_WhenOneLineWithoutClassesWithFields()
        {
            var list = new List<object>() { 1, 2, 3, 5, 6, 7, 8, 0, 9, 6, 4, 3, 5, 6, 8 };
            AssertThatListContainCollection(list,"[1, 2, 3, 5, 6, 7, 8, 0, 9, 6, 4, 3, 5, 6, 8]");
        }
        [Test]
        public void PrintListCollection_WhenHaveNesting()
        {
            var list = new List<object> { 1, 2, 3, new List<int>() { 1, 2, 3 }, new List<string> { "a,e", "something" } };
            AssertThatListContainCollection(list, "[1, 2, 3, [1, 2, 3], [a,e, something]]");
        }
        [Test]
        public void PrintListCollection_WhenHaveNestingDictionary()
        {
            var list = new List<object> { new Dictionary<int, string>() { [1] = "edee", [2] = "rffe", [5] = "fffff" }, 1, 2, 3, new List<int>() { 1, 2, 3 }, new List<string> { "a,e", "something" } };
            AssertThatListContainCollection(list, "[[[1] = edee, [2] = rffe, [5] = fffff], 1, 2, 3, [1, 2, 3], [a,e, something]]");
        }
        [Test]
        public void PrintListCollection_WhenHaveNestingArray()
        {
            var list = new List<object> { new[] { 1, 2, 3, 4 }, new[] { 2, 3, 5, 5, 45, 4, 54, 54 }, new[] { 1336, 1337, 1338 } };
            AssertThatListContainCollection(list, "[[1, 2, 3, 4], [2, 3, 5, 5, 45, 4, 54, 54], [1336, 1337, 1338]]");
        }
        [Test]
        public void PrintListCollection_WhenHaveNestingOtherIEnumerableCollection()
        {
            var list = new List<object> { new HashSet<int>() { 1, 2, 3, 4 }, new HashSet<int>() { 2, 3, 5, 5, 45, 4, 54, 54 }, new HashSet<int>() { 1336, 1337, 1338 } };
            AssertThatListContainCollection(list, "[[1, 2, 3, 4], [2, 3, 5, 45, 4, 54], [1336, 1337, 1338]]");//Cant repeat([[1, 2, 3, 4], [2, 3, 5, 5, 45, 4, 54, 54], [1336, 1337, 1338]]) because HashSet 
        }
        [Test]
        public void PrintListCollection_WhenHaveClassWithFields()
        {
            var list = new List<object> { new Person() { Age = 22, Name = "Sirgey" } };
            AssertThatListContainCollection(list, "[Person; Name = Sirgey; Age = 22; HaveCar = False]");
        }
        [Test]
        public void PrintListCollection_WhenHaveClassesWithFields()
        {
            var list = new List<object> { new Person() { Age = 22, Name = "Sirgey" }, new Person() { Age = 24, Height = 155, Name = "Anna" } };
            AssertThatListContainCollection(list, "[Person; Name = Sirgey; Age = 22; HaveCar = False, Person; Name = Anna; Height = 155; Age = 24; HaveCar = False]");
        }
        [Test]
        public void PrintListCollection_WhenHaveClassWithClassField()
        {
            var list = new List<object> { new Person { Age = 22, Name = "Sirgey", Car = new Vehicle("BMW", 230, 1500, 2003) } };
            AssertThatListContainCollection(list, "[Person; Name = Sirgey; Age = 22; HaveCar = True; Car = Vehicle; CarNumber = 0; BrandAuto = BMW; Power = 230; Weight = 1500; AgeOfCar = 19]");
        }
    }
}
