using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterTests
    {
        [Test]
        public void For_ShouldReturnConfigWithSameGenericType()
        {
            ObjectPrinter.For<Person>().GetType().Should().Be(new PrintingConfig<Person>().GetType());
        }
    }
}