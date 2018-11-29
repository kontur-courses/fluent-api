using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    class PrintingConfigTests
    {
        [Test]
        public void ExcludeShouldAddPropertyToExcludedPropertiesSet()
        {
            var printingConfig = new PrintingConfig<Person>();
            printingConfig.Exclude<int>();

            printingConfig.Settings.GetExcludedProperties().Should().Contain(typeof(int));
        }

    }
}
