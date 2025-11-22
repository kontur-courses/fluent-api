using System.Collections.Generic;
using System.Globalization;
using ApprovalTests;
using ApprovalTests.Namers;
using ApprovalTests.Reporters;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    [UseApprovalSubdirectory("ApprovalTests")]
    public class ObjectPrinterAcceptanceTests
    {
        private Family family;
        
        [SetUp]
        public void SetUp()
        {
            family = new Family
            {
                Father = new Person { Name = "Alex", Age = 19 },
                Mother = new Person { Name = "Kate", Age = 21 },
                Age = 55335.57d,
                GrandParents = [new Person { Name = "Alex", Age = 19 }, new Person { Name = "Kate", Age = 21 }],
                Parents = [new Person { Name = "Alex", Age = 19 }, new Person { Name = "Kate", Age = 21 }],
                Children = new Dictionary<int, Person>
                {
                    [1] = new() { Name = "Alex", Age = 19 },
                    [2] = new() { Name = "Kate", Age = 21 }
                }
            };
        }
        
        [Test]
        public void PrintToString_ReturnsString_WhenLotOfRules()
        {
            var printer = ObjectPrinter.For<Family>()
                .Serialize<int>(x => $"{x}INT:")
                .Using(x => x.Age).Serialize(x => $"A{x}:")
                .Using<string>().Trim(2)
                .Exclude(x => x.Father.Age);
            var result = printer.PrintToString(family);
            
            Approvals.Verify(result);
        }
        
        [Test]
        public void PrintToString_ReturnsString_WhenNoAdditionalRules()
        {
            var printer = ObjectPrinter.For<Family>();
            var result = printer.PrintToString(family);
            
            Approvals.Verify(result);
        }
        
        [Test]
        public void PrintToString_ReturnsString_WhenExcludingType()
        {
            var printer = ObjectPrinter.For<Family>().Exclude<int>();
            var result = printer.PrintToString(family);
            
            Approvals.Verify(result);
        }
        
        [Test]
        public void PrintToString_ReturnsString_WhenSerializationsProperties()
        {
            var printer = ObjectPrinter.For<Family>().Serialize<int>(x => $"NUMBER {x}");
            var result = printer.PrintToString(family);
            
            Approvals.Verify(result);
        }
        
        [Test]
        public void PrintToString_ReturnsString_WhenExcludingProperty()
        {
            var printer = ObjectPrinter.For<Family>().Exclude(x => x.Father.Age);
            var result = printer.PrintToString(family);
            
            Approvals.Verify(result);
        }
        
        [Test]
        public void PrintToString_ReturnsString_WhenWhenSettingCulture()
        {
            var printer = ObjectPrinter.For<Family>().Using<int>().SetCulture(CultureInfo.CurrentCulture);
            var result = printer.PrintToString(family);
            
            Approvals.Verify(result);
        }
        
        [Test]
        public void PrintToString_ReturnsString_WhenTrimmingProperties()
        {
            var printer = ObjectPrinter.For<Family>().Using<string>().Trim(3);
            var result = printer.PrintToString(family);
            
            Approvals.Verify(result);
        }
        
        [Test]
        public void PrintToString_ReturnsString_WhenSerializationsProperty()
        {
            var printer = ObjectPrinter.For<Family>().Using(x => x.Father.Name).Serialize(x => $"PROPERTY {x}");
            var result = printer.PrintToString(family);
            
            Approvals.Verify(result);
        }
        
        [Test]
        public void PrintToString_ReturnsString_WhenUsingExtension()
        {
            var result = family.PrintToString();
            
            Approvals.Verify(result);
        }
        
        [Test]
        public void PrintToString_ReturnsString_WhenUsingExtensionWithConfig()
        {
            var result = family.PrintToString(x => x.Exclude<string>().Exclude<int>());
            
            Approvals.Verify(result);
        }
        
        [Test]
        public void PrintToString_ReturnsString_WhenTrimmingProperty()
        {
            var printer = ObjectPrinter.For<Family>().Using(x => x.Father.Name).Trim(3);
            var result = printer.PrintToString(family);
            
            Approvals.Verify(result);
        }
    }
}