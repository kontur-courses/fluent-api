using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;
using ObjectPrinting.Solved;

namespace ObjectPrintingTests.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        [Test]
        public void PrintToString_FullConfiguration_AppliesAllSettings()
        {
            var account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "VeryLongName123",
                Balance = 1.5,
                Score = 19,
                Age = 25,
            };

            var printer = ObjectPrinter
                .For<Account>()
                .Excluding<Guid>()
                .Printing<int>()
                    .Using(value => value.ToString("X"))
                .Printing<double>()
                    .Using(CultureInfo.InvariantCulture)
                .Printing(x => x.Name)
                    .TrimmedToLength(10)
                .Excluding(x => x.Age);

            var withConfig = printer.PrintToString(account);

            withConfig.Should().Contain("Account");
            withConfig.Should().NotContain("Id");
            withConfig.Should().Contain("Score = 13");
            withConfig.Should().Contain("Balance = 1.5");
            withConfig.Should().Contain("Name = VeryLongNa");
            withConfig.Should().NotContain("VeryLongName123");
            withConfig.Should().NotContain("Age");
        }

        [Test]
        public void PrintToString_Default_DoesNotApplyConfiguration()
        {
            var account = new Account
            {
                Id = Guid.NewGuid(),
                Name = "VeryLongName123",
                Balance = 1.5,
                Score = 19,
                Age = 25,
            };

            var withDefault = account.PrintToString();

            withDefault.Should().Contain("Id =");
            withDefault.Should().Contain("Age = 25");
            withDefault.Should().Contain("Name = VeryLongName123");
            withDefault.Should().Contain("Score = 19");
        }

        [Test]
        public void PrintToString_WithConfigureDelegate_AppliesConfiguration()
        {
            var account = new Account
            {
                Name = "VeryLongName123",
                Age = 25,
            };

            var withDelegate = account.PrintToString(config => config.Excluding(x => x.Age));

            withDelegate.Should().Contain("Name = VeryLongName123");
            withDelegate.Should().NotContain("Age");
        }
    }
}