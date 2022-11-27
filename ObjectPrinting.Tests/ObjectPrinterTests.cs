using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Children)]
    public class ObjectPrinterTests
    {
        [Test]
        public void Should_SerializeCollections()
        {
            var list = new List<string> { "a", "b", "c", "d", "d", "f" };
            var printer = ObjectPrinter.For<List<string>>();
            Console.Out.WriteLine(printer.PrintToString(list));
        }
    }
}