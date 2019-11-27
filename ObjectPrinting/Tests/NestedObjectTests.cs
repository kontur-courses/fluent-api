using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class NestedObjectTests_Should
    {
        public PrintingConfig<NestedObjectContainer> Printer;
        public NestedObjectContainer Container = new NestedObjectContainer();

        [SetUp]
        public void SetUp()
        {
            Printer = ObjectPrinter.For<NestedObjectContainer>();
        }
        [Test]
        public void StopUnwrappingObject_WhenNestingLevelGreaterThanDefault()
        {
            Printer.PrintObject(Container).Should().Contain("...");
        }
        [Test]
        public void StopUnwrappingObject_WhenNestingLevelGreaterThanGiven()
        {
            Printer.UsingMaxRecursionLevel(10).PrintObject(Container).Should().Contain("...");
        }
    }
}