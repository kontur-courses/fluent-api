using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Extensions;

namespace ObjectPrinting.Tests;

[TestFixture]
internal class BasicPrintingTests : PersonFixture
{
    [Test]
    public void ShouldPrintFinalTypesAndNestedObject()
    {
        var result = person.PrintToString();

        result.Should()
            .Contain($"{nameof(Person.Name)} = {person.Name}")
            .And.Contain($"{nameof(Person.Age)} = {person.Age}")
            .And.Contain($"{nameof(Person.Address.City)} = {person.Address.City}");
    }

    [Test]
    public void ShouldHandleNull()
    {
        Person nullPerson = null;

        nullPerson.PrintToString().Should().Be("null" + Environment.NewLine);
    }

    [Test]
    public void ShouldPrintEnum()
    {
        var result = person.PrintToString();

        result.Should().Contain($"{nameof(Person.Status)} = {person.Status}");
    }
        
    [Test]
    public void ShouldPrintGuid()
    {
        var result = person.PrintToString();

        result.Should().Contain($"{nameof(Person.Id)} = {person.Id}");
    }
        
}