using System.Globalization;
using NUnit.Framework;
using ObjectPrinting;

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
            person = new Person { Name = "Alex", Age = 19 };
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

            var actual = printer.Serialize<double>().Using(num => (num + 1).ToString(CultureInfo.InvariantCulture))
                .PrintToString(person);

            Assert.AreEqual(expected, actual);
        }

        [Test] //Пока не понял, как сделать чтобы тест сваливался, поэтому не сделал реализацию.
        public void SerializeOnlyDigitTypeWithSelectedCulture_ShouldSerialize()
        {
            var expected = $"Person\r\n\tId = Guid\r\n\tName = {person.Name}\r\n\tHeight = {person.Height}" +
                           $"\r\n\tAge = {person.Age.ToString(CultureInfo.InvariantCulture)}\r\n";

            var actual = printer.Serialize<int>().Using(CultureInfo.InvariantCulture).PrintToString(person);

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
        public void ExcludeSelectedProperty()
        {
            var expected =
                $"Person\r\n\tId = Guid\r\n\tName = ex\r\n\tHeight = {person.Height}\r\n";

            var actual = printer.Serialize(p => p.Age).Exclude().PrintToString(person);

            Assert.AreEqual(expected, actual);
        }
    }
}
