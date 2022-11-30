namespace ObjectPrintingTests
{
    public class TrimStringShould
    {
        private string printedString;
        public Vehicle VehicleCar;
        private Person simplePerson;
        private Person childPerson;
        private PrintingConfig<Person> printer;

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
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

        [Test]
        public void PrintString_WhenTrimStringField()
        {
            printer.Printing(x => x.Name).TrimmedToLength(4);
            printedString = printer.PrintToString(childPerson);
            printedString.Should().Contain("Sera").And.NotContain("Seraphina");
        }

        [Test]
        public void PrintString_WhenTrimStringBiggerThanLength()
        {
            printer.Printing(x => x.Name).TrimmedToLength(55);
            printedString = printer.PrintToString(childPerson);
            printedString.Should().Contain("Seraphina");
        }

        [Test]
        public void PrintString_WhenTrimStringBiggerEqualLength()
        {
            printer.Printing(x => x.Name).TrimmedToLength(9);
            printedString = printer.PrintToString(childPerson);
            printedString.Should().Contain("Seraphina");
        }

        [Test]
        public void ThrowException_whenTrimmedLengthIsNegative()
        {
            var printer = ObjectPrinter.For<string>();
            Action action = () => { printer.Printing<string>().TrimmedToLength(-1); };
            action.Should().Throw<ArgumentOutOfRangeException>();
        }
    }
}
