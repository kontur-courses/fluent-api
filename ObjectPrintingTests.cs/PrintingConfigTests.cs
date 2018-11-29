using System.Globalization;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.SerializingConfig;

namespace ObjectPrintingTests.cs
{
    [TestFixture]
    public class PrintingConfigTests
    {
        private PrintingConfig<Person> printer;
        private Person person;

        [SetUp]
        public void SetUp()
        {
            printer = ObjectPrinter.For<Person>();
            person = new Person { Name = "Alex", Age = 19, Height = 12.3};
        }

        [Test]
        public void ExcludeSelectedPropertyType()
        {
            var expected = $"Person\r\n\tId = Guid\r\n\tName = {person.Name}\r\n\tHeight = {person.Height}\r\n";

            var actual = printer.Exclude<int>().PrintToString(person);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SerializeSelectedPropertyTypeAlternativly()
        {
            var expected =
                $"Person\r\n\tId = Guid\r\n\tName = {person.Name}\r\n\tHeight = {person.Height + 1}\r\n\tAge = {person.Age}\r\n";

            var actual = printer.Serialize<double>().Using(num => ((double)num + 1).ToString())
                .PrintToString(person);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SerializeOnlyDigitTypeWithSelectedCulture_ShouldSerialize()
        {
            var expected = $"Person\r\n\tId = Guid\r\n\tName = {person.Name}\r\n\tHeight = " +
                           $"{person.Height.ToString(CultureInfo.InvariantCulture)}\r\n\tAge = {person.Age}\r\n";

            var actual = printer.Serialize<double>().Using(CultureInfo.InvariantCulture).PrintToString(person);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SerializeSelectedProperty()
        {
            var expected =
                $"Person\r\n\tId = Guid\r\n\tName = {person.Name}\r\n\tHeight = {person.Height}\r\n\tAge = haha\r\n";

            var actual = printer.Serialize(p => p.Age).Using(p => "haha").PrintToString(person);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CutStringProperty()
        {
            var expected =
                $"Person\r\n\tId = Guid\r\n\tName = ex\r\n\tHeight = {person.Height}\r\n\tAge = {person.Age}\r\n";

            var actual = printer.Serialize<string>().Cut(2).PrintToString(person);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void DefaultSerialize()
        {
            var expected =
                $"Person\r\n\tId = Guid\r\n\tName = {person.Name}\r\n\tHeight = {person.Height}\r\n\tAge = {person.Age}\r\n";

            var changedConfig = printer.Exclude(p => p.Age).Serialize<string>().Using(s => $"#{s}#");
            var actual = changedConfig.DefaultSerialize().PrintToString(person);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ExcludeSelectedProperty()
        {
            var expected =
                $"Person\r\n\tId = Guid\r\n\tName = {person.Name}\r\n\tHeight = {person.Height}\r\n";

            var actual = printer.Exclude(p => p.Age).PrintToString(person);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void SerializationWithConfiguration()
        {
            var expected =
                $"Person\r\n\tName = {person.Name}\r\n\tHeight = {person.Height}\r\n\tAge = {person.Age}\r\n";

            var actual = person.PrintToString(s => s.Exclude(p => p.Id));

            Assert.AreEqual(expected, actual);
        }
    }
}
