using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void Demo()
        {
            var person = PersonFactory.Get();
            var printer = ObjectPrinter
                .For<Person>()
                .Exclude<int>()
                .When<int>()
                    .Use(value => $"~{value}~");
            printer.PrintToString(person);
        }
    }
}