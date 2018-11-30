using FluentAssertions;
using NUnit.Framework;


namespace ObjectPrinting.Tests
{
    [TestFixture()]
    class PropertySerializationConfigTests
    {
        [TestCase(TestName = "SetsNewSerializationFuncForProperty")]
        public void UsingShould()
        {
            var printingConfig = new PrintingConfig<Person>()
                .Serialize(p => p.Age).Using(l => "result of serialization changing");

            ((ISettings)printingConfig).Settings.GetPropertiesSerialization()["Age"](typeof(int))
                .Should().Be("result of serialization changing");
        }
    }
}
