using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    class TypePrintingConfigTests
    {
        [TestCase(TestName = "AddNewSerializationMethodForProperty")]
        public void UsingShould()
        {
            var printingConfig = new PrintingConfig<Person>()
                .Serialize<int>().Using(t => t.Name);

            printingConfig.Settings.GetChangedTypesSerialization().ContainsKey(typeof(int));
        }

        [Test]
        public void UsingShouldUpdateFunctionValueForSameType()
        {
            var printingConfig = new PrintingConfig<Person>()
                .Serialize<int>().Using(t => t.Name);

            printingConfig
                .Serialize<int>().Using(t => "new value");

            printingConfig.Settings
                .GetChangedTypesSerialization()[typeof(int)](typeof(int))
                .Should().Be("new value");
        }
    }
}