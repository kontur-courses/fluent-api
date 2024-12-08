using FluentAssertions;
using ObjectPrinting.Utilits.Strings;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [TestCase("abcd", "ab", "cd")]
        [TestCase("abcd", "", "abcd")]
        public void TrimEnd_TrimsCorrectly(string line, string expected, string toTrim)
        {
            var actual = line.TrimEnd(toTrim);
            actual.Should().Be(expected);
        }
    }
}