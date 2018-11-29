using System;
using System.Linq;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    public class MemberInfoExtensions_Should
    {
        [Test]
        public void Throws_WhenCheckedInappropriateMemberInfo()
        {
            Assert.Throws<ArgumentException>(() =>
                typeof(string).GetMember("ToString").First().CheckCanParticipateInSerialization());
        }

        [Test]
        public void NotThrows_WhenCheckedAppropriateMemberInfo()
        {
            Assert.DoesNotThrow(() =>
                typeof(Person).GetMember("Age").First().CheckCanParticipateInSerialization());
        }
    }
}
