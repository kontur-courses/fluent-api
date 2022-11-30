namespace ObjectPrintingTests
{
    public class SerializationShould
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
        public void PrintPersonWithSpecialSerialization_WhenTypeString()
        {
            printer.Printing<int>().Using(x => $"{x} {x.GetType()}");
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().Contain("System.Int32");
        }

        [Test]
        public void PrintPersonWithSpecialSerialization_WhenTypeInt()
        {
            printer.Printing<string>().Using(x => $"{x} {x.GetType()}");
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().Contain("System.String");
        }

        [Test]
        public void PrintPersonWithSpecialSerialization_WhenTypeDouble()
        {
            printer.Printing<double>().Using(x => $"{x} {x.GetType()}");
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().Contain("System.Double");
        }

        [Test]
        public void PrintPersonWithSpecialFieldSerialization_WhenPropertyNameString()
        {
            printer.Printing(x => x.Name).Using(x => $"{x}(Official form)");
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().Contain("Anna(Official form)");
        }

        [Test]
        public void PrintPersonWithSpecialFieldSerialization_WhenPropertyAgeInt()
        {
            printer.Printing(x => x.Age).Using(x => $"{x} years");
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().Contain("Age = 35 years");
        }

        [Test]
        public void PrintPersonWithSpecialFieldSerialization_WhenPropertyHeightDouble()
        {
            printer.Printing(x => x.Height).Using(x => $"{x} cm");
            printedString = printer.PrintToString(simplePerson);
            printedString.Should().Contain("Height = 155 cm");
        }
    }
}
