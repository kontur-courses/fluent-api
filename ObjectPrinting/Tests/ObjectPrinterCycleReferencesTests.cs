using NUnit.Framework;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterCycleReferencesTests
    {
        [Test]
        public void ObjectPrinter_TzarPrintingCycleReference()
        {
            var tzar1 = new Tzar("Serega", 18);
            var tzar2 = new Tzar("Kolya", 74);
            var tzar3 = new Tzar("Ekaterina", 45);
            tzar1.PreviousTzar = tzar2;
            tzar1.NextTzar = tzar3;
            tzar2.PreviousTzar = tzar1;
            var printer = new PrintingConfig<Tzar>();
            var str = printer.PrintToString(tzar1);
            str.Should().Be("Tzar\r\n	Age = 18\r\n	Name = Serega\r\n	PreviousTzar = Tzar\r\n		Age = 74\r\n		Name = Kolya\r\n		NextTzar = null\r\n		Id = 1\r\n	NextTzar = Tzar\r\n		Age = 45\r\n		Name = Ekaterina\r\n		PreviousTzar = null\r\n		NextTzar = null\r\n		Id = 2\r\n	Id = 0\r\n");
        }
    }
}
