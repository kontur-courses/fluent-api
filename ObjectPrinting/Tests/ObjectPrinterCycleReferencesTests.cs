using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using FluentAssertions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    class ObjectPrinterCycleReferencesTests
    {
        [Test]
        public void ObjectPrinter_TzarPrintingCycleReference()
        {
            var tzar1 = new Tzar("Serega", 18, null, null);
            var tzar2 = new Tzar("Kolya", 74, null, null);
            var tzar3 = new Tzar("Valya", 45, null, null);
            tzar1.Parent1 = tzar2;
            tzar1.Parent2 = tzar3;
            tzar2.Parent1 = tzar1;
            var printer = new PrintingConfig<Tzar>();
            var str = printer.PrintToString(tzar1);
            str.Should().Be("Tzar\r\n	Age = 18\r\n	Name = Serega\r\n	Parent1 = Tzar\r\n		Age = 74\r\n		Name = Kolya\r\n		Parent2 = null\r\n		Id = 1\r\n	Parent2 = Tzar\r\n		Age = 45\r\n		Name = Valya\r\n		Parent1 = null\r\n		Parent2 = null\r\n		Id = 2\r\n	Id = 0\r\n");
        }
    }
}
