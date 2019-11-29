using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    class MemberTests
    {
        private readonly PersonForTests person = new PersonForTests { Name = "Alex", Height = 175, Age = 19 };

        [Test]
        public void Constructor_ShouldThrowArgumentException_IfArgumentIsMethodInfo()
        {
            var methodInfo = typeof(PersonForTests).GetMethod("Do");
            Action action = () => new Member(methodInfo, person);

            action.ShouldThrow<ArgumentException>();
        }

        [Test]
        public void Constructor_ShouldThrowArgumentException_IfArgumentIsConstructorInfo()
        {
            var constructorInfo = typeof(PersonForTests).GetConstructor(new Type[0]);
            Action action = () => new Member(constructorInfo, person);

            action.ShouldThrow<ArgumentException>();
        }
    }
}
