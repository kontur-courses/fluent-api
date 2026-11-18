using System;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    internal class TrimmedToLengthTests : PersonFixture
    {
        [Test]
        public void ShouldTruncateLongStrings()
        {
            var result = person.PrintToString(cfg =>
                cfg.Printing(p => p.Name).TrimmedToLength(2));

            result.Should().Contain("Name = Al").And.NotContain("Name = Alex");
        }

        [Test]
        public void ShouldThrowOnNegativeLength()
        {
            Action act = () => person.PrintToString(cfg =>
                cfg.Printing(p => p.Name).TrimmedToLength(-1));

            act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("maxLen");
        }
    }
}