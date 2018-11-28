using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting;
using ObjectPrinting.Tests;

namespace ObjectPrintingTests
{
    [TestFixture]
    public class ObjectPrinter_Should
    {
        [Test]
        public void ExcludeFieldByType()
        {
            var obj = new Person();
            var expected = "Person\r\n\tId = Guid\r\n\tName = null\r\n\tHeight = 0\r\n";

            var result = obj.PrintToStr(o => o.Exclude<int>());

            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void ExcludeFieldByLambda()
        {
            var obj = new Person();
            var expected = "Person\r\n\tId = Guid\r\n\tHeight = 0\r\n\tAge = 0\r\n";

            var result = obj.PrintToStr(o => o.Exclude(p => p.Name));

            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void SerializeByType()
        {
            var obj = new Person();
            var expected = "Person\r\n\tId = Guid\r\n\tName = null\r\n\tHeight = 0\r\n\tAge = <int> 0\r\n";

            var result = obj.PrintToStr(o => o.Serialize<int>().As(p => $"<int> {p}"));

            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void SerializeByLambda()
        {
            var obj = new Person();
            var expected = "Person\r\n\tId = Guid\r\n\tName = null\r\n\tHeight = <double> 0\r\n\tAge = 0\r\n";

            var result = obj.PrintToStr(o => o.Serialize(p => p.Height).As(p => $"<double> {p}"));

            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public void SetCulture()
        {
            var obj = new Person();
            obj.Height = 123.45d;
            var expected = "Person\r\n\tId = Guid\r\n\tName = null\r\n\tHeight = 123.45\r\n\tAge = 0\r\n";

            var result = obj.PrintToStr(o => o.SetCultureFor<double>(CultureInfo.InvariantCulture));

            result.Should().BeEquivalentTo(expected);
        }
    }
}
