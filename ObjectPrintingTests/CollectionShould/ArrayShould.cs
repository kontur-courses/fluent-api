using Newtonsoft.Json.Linq;
using ObjectPrinting;
using ObjectPrintingTests.TestClasses;
using System;

namespace ObjectPrintingTests.CollectionShould
{
    [TestFixture]
    public class ArrayShould
    {
        private string printedString;
        public Vehicle VehicleCar;
        private Person simplePerson;
        private Person childPerson;
        private PrintingConfig<object[]> printer;

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


        public void AssertThatArrayContainCollection(object[] arrayWithObjects, string shouldContainInResult)
        {
            printer = ObjectPrinter.For<object[]>();
            printedString = printer.PrintToString(arrayWithObjects);
            printedString.Should().Contain(shouldContainInResult);
        }

        [Test]
        public void PrintArrayCollection_WhenOneLineWithoutClassesWithFields()
        {
            var array = new object[] { 1, 2, 3, 5, 6, 7, 8, 0, 9, 6, 4, 3, 5, 6, 8 };
            AssertThatArrayContainCollection(array, "[1, 2, 3, 5, 6, 7, 8, 0, 9, 6, 4, 3, 5, 6, 8]");
        }

        [Test]
        public void PrintArrayCollection_WhenHaveNesting()
        {
            var array = new object[] { 1, 2, 3, new List<int>() { 1, 2, 3 }, new List<string> { "a,e", "something" } };
            AssertThatArrayContainCollection(array, "[1, 2, 3, [1, 2, 3], [a,e, something]]");
        }

        [Test]
        public void PrintArrayCollection_WhenHaveNestingDictionary()
        {
            var array = new object[]
            {
                new Dictionary<int, string>() { [1] = "edee", [2] = "rffe", [5] = "fffff" }, 1, 2, 3,
                new List<int>() { 1, 2, 3 }, new List<string> { "a,e", "something" }
            };
            AssertThatArrayContainCollection(array,
                "[[[1] = edee, [2] = rffe, [5] = fffff], 1, 2, 3, [1, 2, 3], [a,e, something]]");
        }

        [Test]
        public void PrintArrayCollection_WhenHaveNestingArray()
        {
            var array = new object[]
                { new[] { 1, 2, 3, 4 }, new[] { 2, 3, 5, 5, 45, 4, 54, 54 }, new[] { 1336, 1337, 1338 } };

            AssertThatArrayContainCollection(array, "[[1, 2, 3, 4], [2, 3, 5, 5, 45, 4, 54, 54], [1336, 1337, 1338]]");
        }

        [Test]
        public void PrintArrayCollection_WhenHaveNestingOtherIEnumerableCollection()
        {
            var array = new object[]
            {
                new HashSet<int>() { 1, 2, 3, 4 }, new HashSet<int>() { 2, 3, 5, 5, 45, 4, 54, 54 },
                new HashSet<int>() { 1336, 1337, 1338 }
            };
            AssertThatArrayContainCollection(array, "[[1, 2, 3, 4], [2, 3, 5, 45, 4, 54], [1336, 1337, 1338]]");
            //Cant repeat([[1, 2, 3, 4], [2, 3, 5, 5, 45, 4, 54, 54], [1336, 1337, 1338]]) because HashSet 
        }

        [Test]
        public void PrintArrayCollection_WhenHaveClassWithFields()
        {
            var persons = new Person[] { new() { Age = 22, Name = "Sirgey" } };
            AssertThatArrayContainCollection(persons, "[Person; Name = Sirgey; Age = 22; HaveCar = False]");
        }

        [Test]
        public void PrintArrayCollection_WhenHaveClassesWithFields()
        {
            var persons = new Person[]
            {
                new Person() { Age = 22, Name = "Sirgey" }, new Person() { Age = 24, Height = 155.3, Name = "Anna" }
            };
            AssertThatArrayContainCollection(persons,
                "[Person; Name = Sirgey; Age = 22; HaveCar = False, Person; Name = Anna; Height = 155,3; Age = 24; HaveCar = False]");
        }

        [Test]
        public void PrintArrayCollection_WhenHaveClassWithClassField()
        {
            var persons = new Person[]
                { new Person { Age = 22, Name = "Sirgey", Car = new Vehicle("BMW", 230, 1500, 2003) } };
            AssertThatArrayContainCollection(persons,
                "[Person; Name = Sirgey; Age = 22; HaveCar = True; Car = Vehicle; CarNumber = 0; BrandAuto = BMW; Power = 230; Weight = 1500; AgeOfCar = 19]");
        }

    }
}
