using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using ObjectPrinting.Tests.TestClasses;

namespace ObjectPrinting.Tests
{
    [TestFixture]
    public class ObjectPrinterAcceptanceTests
    {
        private PrintingConfig<Person> printingConfig;
        
        private readonly Person person = new Person 
        {
            Child = new Person()
            {
                Name = "Volodya",
                Age = 2
            },
            Car = new Car()
            {
                Color = 123,
                Company = "Tesla",
                Name = "My lovely car"
            },
            Company = "Kontur", Name = "Alex", Surname = "Tsvetkov", Age = 19, Height = 5.6, Width = 7.2f };

        private readonly object[] objectArray = { new Person(), "322", new Car() { Color = 225 } };
        private readonly List<int> intList = new List<int>() { 1, 2, 3 };
        private readonly Dictionary<int, int> dictionary = new Dictionary<int, int>()
        {
            { 1, 2 },
            { 2, 3 },
            { 3, 4 }
        };

        [SetUp]
        public void SetUp()
        {
            printingConfig = ObjectPrinter.For<Person>();
        }

        [Test]
        public void Demo()
        {
            var personPrinter = ObjectPrinter.For<Person>()
                .Excluding(x => x.Age)
                .Printing(x => x.Name).Using(x => x.ToString() + "BBB")
                .Printing(x => x.Car.Company).Using(x => "COMPANY")
                .Printing<float>().Using(CultureInfo.InvariantCulture)
                .Printing<double>().Using(x => (x + 100).ToString())
                .Printing(x => x.Name).TrimmedToLength(50);

            var s3 = personPrinter.PrintToString(person);
            Console.WriteLine(s3);
            
            var s4 = person.PrintToString();
            Console.WriteLine(s4);
            
            var s5 = person.PrintToString(x => x.Excluding(x => x.Age));
            Console.WriteLine(s5);
            
            Console.WriteLine(objectArray.PrintToString());
            Console.WriteLine(intList.PrintToString());
            Console.WriteLine(dictionary.PrintToString());
        }

        [Test]
        public void PrintingConfig_ImmutableObject()
        {
            var printer = ObjectPrinter.For<Person>().Printing<int>().Using(x => "AAAAAA");
            var printer1 = printer.Printing<float>().Using(x => "BBBBBBB");;
            var printer2 = printer.Printing<double>().Using(x => "CCCCCCCC");;

            var s1 = printer.PrintToString(person);
            var s2 = printer1.PrintToString(person);
            var s3 = printer2.PrintToString(person);
            
            StringAssert.AreNotEqualIgnoringCase(s1, s2);
            StringAssert.AreNotEqualIgnoringCase(s1, s3);
            StringAssert.AreNotEqualIgnoringCase(s2, s3);
        }

        [Test]
        public void PrintingConfig_WithExcludeType_ShouldReturnString()
        {
            var personConfig = printingConfig.Excluding<Guid>();

            var result = personConfig.PrintToString(person);
            
            StringAssert.DoesNotContain("Id", result);
        }
        
        [Test]
        public void PrintingConfig_WithExcludeProperty_ShouldReturnString()
        {
            var personConfig = printingConfig.Excluding(x => x.Age);

            var result = personConfig.PrintToString(person);
            
            StringAssert.DoesNotContain("Age", result);;
        }
        
        [Test]
        public void PrintingConfig_WithCustomSerializeType_ShouldReturnString()
        {
            var personConfig = printingConfig.Printing<Guid>().Using(x => "id111");

            var result = personConfig.PrintToString(person);
            
            StringAssert.DoesNotContain("Guid", result);
        }
        
        [Test]
        public void PrintingConfig_WithCustomSerializeProperty_ShouldReturnString()
        {
            var personConfig = printingConfig.Printing(x => x.Name).Using(x => "IDK");

            var result = personConfig.PrintToString(person);
            
            StringAssert.DoesNotContain(person.Name, result);
        }
        
        [Test]
        public void PrintingConfig_WithTrimStringProperty_ShouldReturnString()
        {
            const int length = 2;
            var subName = person.Name[..length];
            var personConfig = printingConfig.Printing(x => x.Name).TrimmedToLength(length);

            var result = personConfig.PrintToString(person);
            
            StringAssert.DoesNotContain(person.Name, result);
            StringAssert.Contains(subName, result);
        }
    }
}