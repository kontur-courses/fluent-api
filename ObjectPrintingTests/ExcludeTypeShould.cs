namespace ObjectPrintingTests
{
    public class ExcludeTypeShould
    {
        private string printedString;
        public Vehicle VehicleCar;
        private Person simplePerson;
        private PrintingConfig<Person> printer;


        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
            VehicleCar = new Vehicle("Audi", 230, 1400, 2005);
            simplePerson = new Person() { Age = 35, Height = 155, Id = new Guid(), Name = "Anna", Car = VehicleCar };
            printedString = null;
        }

        [TearDown]
        public void TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status != TestStatus.Failed)
                Console.WriteLine(printedString);
        }

        [Test]
        public void PrintPersonWithoutStringProperties_WhenExcludeStringType()
        {
            printer = printer.Excluding<string>();
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("BrandAuto");
        }

        [Test]
        public void PrintPersonWithoutGuidProperties_WhenExcludeGuidType()
        {
            printer = printer.Excluding<Guid>();
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("Id");
        }

        [Test]
        public void PrintPersonWithoutIntProperties_WhenExcludeIntType()
        {
            printer = printer.Excluding<int?>();
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("Age");
        }

        [Test]
        public void PrintPersonWithoutDoubleProperties_WhenExcludeDoubleType()
        {
            printer = printer.Excluding<double?>();
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("Weight");
        }

        [Test]
        public void PrintPersonWithoutBoolProperties_WhenExcludeBoolType()
        {
            printer = printer.Excluding<bool>();
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("HaveCar");
        }

        [Test]
        public void PrintPersonWithoutListProperties_WhenExcludeListType()
        {
            printer = printer.Excluding<List<int>>();
            simplePerson.TypeList = new List<int> { 1, 2, 3 };
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("TypeList");

        }

        [Test]
        public void PrintPersonWithoutDictionaryProperties_WhenExcludeDictionaryType()
        {
            printer = printer.Excluding<Dictionary<int, object>>();
            simplePerson.TypeDict = new Dictionary<int, object>() { [1] = "Car", [2] = 2123.3 };
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("TypeDict");
        }

        [Test]
        public void PrintPersonWithoutIEnumerableOtherCollectionProperties_WhenExcludeIEnumerableOtherCollectionType()
        {
            printer = printer.Excluding<HashSet<string>>();
            simplePerson.TypeSet = new HashSet<string> { "something text", "for hashset test" };
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("TypeSet");
        }

        [Test]
        public void PrintPersonWithoutArrayProperties_WhenExcludeArrayType()
        {
            printer = printer.Excluding<int[]>();
            simplePerson.TypeArray = new int[] { 1, 2, 3 };
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("TypeArray");
        }

        [Test]
        public void PrintPersonWithoutCustomClassProperties_WhenExcludeCustomType()
        {
            printer = printer.Excluding<Vehicle>();
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().NotContain("Vehicle");
        }
    }
}
