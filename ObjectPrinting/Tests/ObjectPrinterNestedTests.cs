using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace ObjectPrinting.Tests;

[TestFixture]
public class ObjectPrinterNestedTests
{
    [Test]
    public void PersonWithNestedCompanyAndAddressTest()
    {
        var person = new Person
        {
            FirstName = "Ivanov",
            Name = "Alex",
            Age = 21,
            Score = 99.9,
            Id = Guid.NewGuid(),
            Company = new Company
            {
                CompanyName = "Kontur",
                Industry = "IT",
                Address = new Address
                {
                    Street = "Maloprudnia 12",
                    City = "EKB",
                    Country = "Russia"
                }
            },
            Address = new Address
            {
                Street = "88",
                City = "Moscow",
                Country = "Russia"
            }
        };

        var printed = ObjectPrinter.For<Person>()
            .Printing<double>().Using(CultureInfo.InvariantCulture)
            .PrintToString(person);

        Assert.That(printed, Contains.Substring("FirstName = Ivanov"));
        Assert.That(printed, Contains.Substring("Name = Alex"));
        Assert.That(printed, Contains.Substring("Age = 21"));
        Assert.That(printed, Contains.Substring("Score = 99.9"));
        Assert.That(printed, Contains.Substring("Id"));

        Assert.That(printed, Contains.Substring("Company"));
        Assert.That(printed, Contains.Substring("CompanyName = Kontur"));
        Assert.That(printed, Contains.Substring("Industry = IT"));

        Assert.That(printed, Contains.Substring("Street = Maloprudnia 12"));
        Assert.That(printed, Contains.Substring("City = EKB"));
        Assert.That(printed, Contains.Substring("Country = Russia"));

        Assert.That(printed, Contains.Substring("Address"));
        Assert.That(printed, Contains.Substring("Street = 88"));
        Assert.That(printed, Contains.Substring("City = Moscow"));
        Assert.That(printed, Contains.Substring("Country = Russia"));
    }
}