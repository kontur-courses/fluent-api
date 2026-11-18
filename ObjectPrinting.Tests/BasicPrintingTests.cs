using System;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    internal class BasicPrintingTests : PersonFixture
    {
        [Test]
        public void ShouldPrintFinalTypesAndNestedObject()
        {
            var result = person.PrintToString();

            result.Should()
                .Contain("Name = Alex")
                .And.Contain("Age = 42")
                .And.Contain("City = Moscow");
        }

        [Test]
        public void ShouldHandleNull()
        {
            Person nullPerson = null;

            nullPerson.PrintToString().Should().Be("null" + Environment.NewLine);
        }

        [Test]
        public void ShouldPrintEnumAndGuid()
        {
            var guid = Guid.NewGuid();
            person.Id = guid;
            person.Status = Status.Active;

            var result = person.PrintToString();

            result.Should().Contain($"Id = {guid}").And.Contain("Status = Active");
        }
    }
}