using ObjectPrinting;
using ObjectPrintingTests.TestData;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void AcceptanceTest()
        {
            var person = new Person(new Guid(), "Alex", 192.8, 33);

            var printer = ObjectPrinter.For<Person>()
                .ExcludeType<int>();
            
            var result = printer.PrintToString(person);
        }
    }
}