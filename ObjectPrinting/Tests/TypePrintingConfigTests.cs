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

            ((ISettings)printingConfig).Settings.GetChangedTypesSerialization().ContainsKey(typeof(int));
        }
    }
}